using Microsoft.AspNet.SignalR;
using SignalR.Todo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private List<TodoItem> _list;
        public TodoController()
        {
            _list = new List<TodoItem>()
            {
                new TodoItem{id=1,title="Hello world",finished=false},
                new TodoItem{id=2,title="Book cab for airport",finished=true},
            };

        }
        IHubContext context = GlobalHost.ConnectionManager.GetHubContext<TodoHub>();
        public List<TodoItem> Get()
        {
            
            return _list;
        }
        public TodoItem Post(TodoItem post)
        {
            _list.Add(post);
            
            context.Clients.All.getTodo(post);
            return post;

        }
        public void Delete(int id)
        {
            var item = _list.Find(x => x.id == id);
            _list.Remove(item);
            context.Clients.All.removeClient(id);

        }
    }
}