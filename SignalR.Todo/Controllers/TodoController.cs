using Microsoft.AspNet.SignalR;
using SignalR.Todo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web;
using System.Web.Http;

namespace SignalR.Todo.Controllers
{
    public class TodoItem
    {
        public int id { get; set; }
        public string title { get; set; }
        public bool finished { get; set; }
    }
    public class TodoController : ApiController
    {

        private List<TodoItem> db;
        private int NextId()
        {
            return db.Max(x => x.id) + 1;
        }
        public TodoController()
        {
            db = new List<TodoItem>()
            {
                new TodoItem{id=1,title="Hello world",finished=false},
                new TodoItem{id=2,title="Book cab for airport",finished=true},
            };

        }
        IHubContext context = GlobalHost.ConnectionManager.GetHubContext<TodoHub>();
        public List<TodoItem> Get()
        {
            lock (db)

                return db;
        }
        public HttpResponseMessage Post(TodoItem todoItem)
        {

            lock (db)
            {
                var nextId = NextId();
                // Add item to the "database"
                todoItem.id = Interlocked.Increment(ref nextId);
                db.Add(todoItem);

                // Notify the connected clients
                context.Clients.All.addTask(todoItem);

                // Return the new item, inside a 201 response
                var response = Request.CreateResponse(HttpStatusCode.Created, todoItem);
                string link = Url.Link("apiRoute", new { controller = "todo", id = todoItem.id });
                response.Headers.Location = new Uri(link);
                return response;
            }

        }
        public TodoItem PutUpdatedToDoItem(int id, TodoItem item)
        {
            lock (db)
            {
                // Find the existing item
                var toUpdate = db.SingleOrDefault(i => i.id == id);
                if (toUpdate == null)
                    throw new HttpResponseException(
                        Request.CreateResponse(HttpStatusCode.NotFound)
                    );
                // Update the editable fields and save back to the "database"
                toUpdate.title = item.title;
                toUpdate.finished = item.finished;

                // Notify the connected clients

                context.Clients.All.updateItem(toUpdate);

                // Return the updated item
                return toUpdate;
            }
        }
        public HttpResponseMessage Delete(int id)
        {
            lock (db)
            {
                int removeCount = db.RemoveAll(i => i.id == id);
                if (removeCount <= 0)
                    return Request.CreateResponse(HttpStatusCode.NotFound);

                // Notify the connected clients

                context.Clients.All.delteTask(id);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            

        }
    }
}