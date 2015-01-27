/// <reference path="jquery-1.9.1.js" />
/// <reference path="jquery.signalR-2.2.0.js" />
/// <reference path="knockout-3.2.0.js" />

var dataService = (function () {
    var url = "/api/todo";
    var get = function () {
        return $.ajax({
            type: "GET",
            url: url,
            dataType: "json",
            contentType: "application/json"
        });
    };
    var post = function (data) {

        return $.ajax({
            type: "POST",
            url: url,
            data: data,
            dataType: "json",
            contentType: "application/json"
        });
    };
    var update = function (data) {
        return $.ajax({
            type: "PUT",
            url: url,
            data: data,
            dataType: "json",
            contentType: "application/json"
        });
    };
    var remove = function (data) {
        return $.ajax({
            type: "DELETE",
            url: '/api/todo/' + data().id,

            dataType: "json",
            contentType: "application/json"
        });
    };
    return {
        addTask: post,
        getTask: get,
        updateTask: update,
        delteTask: remove

    };

})();
function TodoItem(id, title, finished) {
    var self = this;
    var updating = true;
    self.id = id;
    self.title = ko.observable(title);
    self.finished = ko.observable(finished);
    self.update = function (title, finished) {
        updating = true;
        self.title(title);
        self.finished(finished);
        updating = false;
    };
    self.remove = function () {
        dataService.delteTask(self);
    };

    self.finished.subscribe(function () {
        if (!updating) {
            dataService.updateTask(self);
        }
    });

};

function ViewModel() {
    var self = this;
    self.newTask = ko.observable();
    self.total = ko.observable();
    self.itemsPending = ko.observable();
    self.hub = $.connection.todoHub;
    self.tasks = ko.observableArray([]);

    self.add = function (id, title, finished) {
        self.tasks.push(new TodoItem(id, title, finished));
    };
    self.removeTask = function (task) {
        self.tasks.remove(task);
    }
    self.total = ko.computed(function () {
        return ko.utils.arrayFilter(self.tasks(), function (item) {
            return item.finished() == true;
        });
    });
    self.addTask = function () {
        var task = new TodoItem(0, self.newTask(), false);
        dataService.addTask(ko.toJSON(task));
    }

    self.itemsPending = ko.computed(function () {
        return ko.utils.arrayFilter(self.tasks(), function (item) {
            return item.finished() == false;
        });
    });
}
$(document).ready(function () {

    var viewModel = new ViewModel(),
        hub = $.connection.todoHub;

    ko.applyBindings(viewModel);

    hub.client.getTodo = function (item) {
        viewModel.add(item.id, item.title, item.finished);
    };
    $.connection.hub.start();

    $.get("/api/todo", function (items) {
        $.each(items, function (idx, item) {
            viewModel.add(item.id, item.title, item.finished);
        });
    }, "json");
});