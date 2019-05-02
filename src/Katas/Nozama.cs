using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using ShoppingCartItems = System.Collections.Generic.IEnumerable<(int, int)>;
using OrderItems = System.Collections.Generic.IEnumerable<(int, int)>;
using ConcurrentInventory = System.Collections.Concurrent.ConcurrentDictionary<int, int>;
using OrderErrors = System.Collections.Generic.IEnumerable<Katas.OrderError>;

namespace Katas
{
#pragma warning disable RCS1194 // Implement exception constructors.

    /* Acceptance Criteria
     * 
     * Create a class that processes Ecommerce orders
     *  
     * Requirements:
     *  Items can be added/updated/removed from the shopping cart
     *  The shopping cart can be cleared
     *  Items can be added/removed from the warehouse
     *  Negative item quantities are not allowed
     *  The user can check out all the items in their cart in their current session
     *  If there are issues fulfilling any part of the order, no part of the order should go through
     *  Items can only be ordered if there is sufficient quantity available in the warehouse
     *  The warehouse should be able to fulfill/deny orders concurrently without locking, and without overselling
     *  After a successful order, the shopping cart should be emptied
     *  If an order is unsuccessful, the shopping cart should remain intact
     *  Orders that cannot be fulfilled should specify which items cannot be fulfilled and why
     *  
     * Testing in isolation:
     *  Task 1: Assume that the Warehouse implementation is using a database and should be mocked for the purposes of unit testing
     *  Task 2: Assume that the shopping cart implementation is using redis and should be mocked for the purposes of unit testing
     * 
     */

    public interface IEcommerce
    {
        void Checkout(in Session session);
    }

    public interface IShoppingCart
    {
        void Add(int itemNumber, int quantity = 1);
        void UpdateQuantity(int itemNumber, int quantity);
        void Remove(int itemNumber);
        void Empty();
        ShoppingCartItems Items { get; }
    }

    public interface IWarehouse
    {
        bool TryFulfill(in Order order, out OrderErrors errors);
        void Add(int itemNumber, int quantity);
        void Remove(int itemNumber, int quantity);
    }

    public readonly ref struct Session
    {
        public readonly Guid ShopperId;
        public readonly IShoppingCart Cart;

        public Session(Guid shopperId, in IShoppingCart cart)
        {
            ShopperId = shopperId;
            Cart = cart;
        }
    }

    public readonly ref struct Order
    {
        public readonly OrderItems Items;

        public Order(in OrderItems items)
        {
            Items = items;
        }
    }

    public class OrderError
    {
        public int ItemNumber { get; }
        public int AvailableQuantity { get; }
        public int RequestedQuantity { get; }

        public OrderError(int itemNumber, int requestedQuantity, int availableQuantity = 0)
        {
            ItemNumber = itemNumber;
            RequestedQuantity = requestedQuantity;
            AvailableQuantity = availableQuantity;
        }

        public override string ToString()
            => $"Item {ItemNumber} was requested with a quantity of {RequestedQuantity}, but only {AvailableQuantity} are available.";
    }

    public class OrderException : ApplicationException
    {
        public OrderErrors Errors { get; }

        public OrderException(string message = default, OrderErrors errors = default, Exception innerException = default)
            : base(message, innerException)
        {
            Errors = errors;
        }

        public override string Message => Errors != null ? string.Join(Environment.NewLine, Errors.Select(e => e.ToString())) : base.Message;
    }

    public class Nozama : IEcommerce
    {
        private readonly IWarehouse _warehouse;

        public Nozama(IWarehouse warehouse)
        {
            _warehouse = warehouse;
        }

        public void Checkout(in Session session)
        {
            var order = new Order(session.Cart.Items);
            if (!_warehouse.TryFulfill(order, out var errors))
            {
                throw new OrderException("Unable to fulfill order", errors);
            }
            session.Cart.Empty();
        }
    }

    public class ShoppingCart : IShoppingCart
    {
        private readonly ConcurrentInventory _items = new ConcurrentInventory();

        public ShoppingCartItems Items
        {
            get
            {
                foreach (var item in _items)
                    yield return (item.Key, item.Value);
            }
        }

        public void Add(int itemNumber, int quantity = 1)
        {
            if (quantity < 1)
                throw new ArgumentOutOfRangeException(nameof(quantity), "Cannot add negative items to your cart");
            _items.AddOrUpdate(itemNumber, quantity, (_, v) => v + quantity);
        }

        public void UpdateQuantity(int itemNumber, int quantity)
        {
            if (quantity < 0)
                throw new ArgumentOutOfRangeException(nameof(quantity), "Cannot add negative items to your cart");
            _items.AddOrUpdate(itemNumber, quantity, (_, __) => quantity);
        }

        public void Remove(int itemNumber)
        {
            _items.TryRemove(itemNumber, out var _);
        }

        public void Empty()
        {
            _items.Clear();
        }
    }

    public class Warehouse : IWarehouse
    {
        public class InsufficientInventoryException : ApplicationException
        {
            public OrderError OrderError { get; }

            public InsufficientInventoryException(int itemNumber, int requested, int available) : base()
            {
                OrderError = new OrderError(itemNumber, requested, available);
            }
        }

        public class InvalidItemException : ApplicationException
        {
            public InvalidItemException() : base("Invalid item") { }
        }

        private readonly ConcurrentInventory _items = new ConcurrentInventory();

        public void Add(int itemNumber, int quantity)
        {
            if (quantity < 0)
                throw new ArgumentOutOfRangeException(nameof(quantity), "Cannot add negative inventory");

            _items.AddOrUpdate(itemNumber, quantity, (_, v) => v + quantity);
        }

        public void Remove(int itemNumber, int quantity)
        {
            if (quantity < 0)
                throw new ArgumentOutOfRangeException(nameof(quantity), "Cannot remove negative inventory");

            SpinWait.SpinUntil(() =>
            {
                if (!_items.TryGetValue(itemNumber, out var available))
                {
                    throw new InvalidItemException();
                }
                if (available - quantity < 0)
                {
                    throw new InsufficientInventoryException(itemNumber, quantity, available);
                }
                return _items.TryUpdate(itemNumber, available - quantity, available);
            });
        }

        public bool TryFulfill(in Order order, out OrderErrors errors)
        {
            var errorList = new List<OrderError>();
            var fulfilled = new List<(int, int)>();
            try
            {
                foreach (var (itemNumber, quantity) in order.Items)
                {
                    try
                    {
                        Remove(itemNumber, quantity);
                        fulfilled.Add((itemNumber, quantity));
                    }
                    catch (InvalidItemException)
                    {
                        errorList.Add(new OrderError(itemNumber, quantity));
                    }
                    catch (InsufficientInventoryException e)
                    {
                        errorList.Add(e.OrderError);
                    }
                }
                if (errorList.Count > 0)
                {
                    // Compensating action
                    foreach (var (itemNumber, quantity) in fulfilled)
                        Add(itemNumber, quantity);
                }
            }
            finally
            {
                errors = errorList;
            }
            return errorList.Count == 0;
        }
    }
}
