using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using api.Models;

namespace api.Controllers
{
    public class ordersController : ApiController
    {
        private DBModel db = new DBModel();

        // GET: api/orders
        public System.Object Getorders()
        {
            var results = (from a in db.orders
                           join b in db.customers on a.customerID equals b.customerID
                           select new
                           {
                               a.orderID,
                               a.orderNo,
                               customer = b.name,
                               a.payment,
                               a.total

                           }).ToList();
            return results;
        }

        // GET: api/orders/5
        [ResponseType(typeof(order))]
        public IHttpActionResult Getorder(long id)
        {
            var order = (from a in db.orders
                         where a.orderID == id
                         select new
                         {
                             a.orderID,
                             a.orderNo,
                             a.customerID,
                             a.payment,
                             a.total,
                             DeletedOrderItemIDs=""

                         }).FirstOrDefault();

            var orderDetails = (from a in db.orderItems
                                join b in db.items on a.itemID equals b.itemID
                                where a.orderID == id
                                select new
                                {
                                    a.orderID,
                                    a.orderItemID,
                                    a.itemID,
                                    itemName = b.name,
                                    b.price,
                                    a.quantity,
                                    total = b.price * a.quantity

                                }

                                ).ToList();
            return Ok(new { order, orderDetails });
        }

        // PUT: api/orders/5
        [ResponseType(typeof(void))]
        public IHttpActionResult Putorder(long id, order order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != order.orderID)
            {
                return BadRequest();
            }

            db.Entry(order).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!orderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/orders
        [ResponseType(typeof(order))]
        public IHttpActionResult Postorder(order order)
        {
            try
            {
                if(order.orderID == 0)
                {
                    db.orders.Add(order);
                }
                else
                {
                    db.Entry(order).State = EntityState.Modified;
                }
                

                foreach (var item in order.orderItems)

                {
                    if(item.orderItemID==0)
                    db.orderItems.Add(item);
                    else

                     db.Entry(item).State = EntityState.Modified;
                }
                //Delete Operations
                foreach (var id in order.DeletedOrderItemIDs.Split(',').Where(x=> x !="") )
                {
                    orderItem x = db.orderItems.Find(Convert.ToInt64(id));
                    db.orderItems.Remove(x);

                }

                db.SaveChanges();

                return Ok();
            }
            catch (Exception)
            {

                throw;
            }

           
        }

        // DELETE: api/orders/5
        [ResponseType(typeof(order))]
        public IHttpActionResult Deleteorder(long id)
        {
            order order = db.orders.Include(y=>y.orderItems)
                .SingleOrDefault(x => x.orderID == id);
            foreach (var item in order.orderItems.ToList())
            {
                db.orderItems.Remove(item);

            }

            db.orders.Remove(order);
            db.SaveChanges();

            return Ok(order);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool orderExists(long id)
        {
            return db.orders.Count(e => e.orderID == id) > 0;
        }
    }
}