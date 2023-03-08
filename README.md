# - 1 -

## Blazor Session Scoped Container

### What is this?

Whenever a user connects to your blazor website, the object instances used inside your Blazor Server App are kept until the user either closes the connections or reconnects (e.g. by pressing F5).

Blazor Session Scoped Dependency Injection allows you' to add services wich instances are kept even if the user disconnects and connects back a while later.

### How does it work?

It basically assigns every visiting user a session id in a form of a cookie. Whenever a service is added, the service instance is associated with that unique session id. The service instances are stored internally in a static dictionary, such that the lifetime of a service object is invariant to the user's connection's/disconnection's.

In order to save memory, every user-specified second/minute/hour/day a garbage collector is activated which filters out inactive user sessions and disposes and removes them.
If the session must not be lost entirely, you can persist the object instance in a json format and save it to the disk.
Whenever the user with that particular session id connects to the website again, the json file will be read, parsed and the previous session will be restored.

### How to use it?

First and foremost add the `IHttpContextAccessor` interface to your blazor app like so

```
builder.Services.AddHttpContextAccessor();
```
Having done that proceed to add the `NSession` service to your blazor with `.AddScoped` like that

```
builder.Services.AddScoped<NSession>();
```

First step **done**!

Whenever you want to create a service class that is invariant to a user's reconnects/disconnects implement the `ISessionScoped` interface in your service. Whenever the session should be restorable even after being disposed by the garbage collector implement `IPersistentSessionScoped` instead.

In order to register the services go to the `_Host.cshtml` file and add following lines

```
@inject NSession Session;

@{
    Session.StartSession((id, handler) =>
    {
        handler.AddService<TestService>(id);
        handler.AddService<ILogger, MyLogger>(id, Session);
    });
}
```

Whenever a user now connects, the registered services will be associated with his unique session id.

In order to notify the user that his session has ended (i.e. the garbage collector considered the user to be inactive) go to the `MainLayout.razor` file and replace it with

```
@using BlazorSessionScopedContainer.RazorComponents;
@using BlazorSessionScopedContainer.Services.Data;
@inherits NSessionMainBase

<PageTitle>BlazorApp1</PageTitle>

<div class="page">
    <div class="sidebar">
        <NavMenu />
    </div>
    <main>
        <div class="top-row px-4">
            <a href="https://docs.microsoft.com/aspnet/" target="_blank">About</a>
        </div>

        @if (Session.GetSession().HasValue)
        {
            <p>Session id: @Session.GetSession()</p>
        }
        else
        {
                <p>No session id obtained.</p>
        }

        @if (this.IsSessionClosed)
        {
            <div style="position: fixed; inset: 0px; z-index: 1050; display: block; overflow: hidden; background-color: rgb(255, 255, 255); opacity: 0.8; text-align: center; font-weight: bold; transition: visibility 0s linear 500ms; visibility: visible;">
                <p>Your session has been closed!</p>
                </div>
        }

        <article class="content px-4">
            @Body
        </article>
    </main>
</div>
```

Notice that this page inherits `NSessionMainBase`, a built-in class that provides a virtual method which is called whenever a notificiation is received:

```
     protected virtual void NotificationReceived(UserSessionNotification sessionNotification)
```


In order to load a registered service in an arbitrary page first inherit `NSessionComponentBase`:

```
@page "/counter"
@using BlazorSessionScopedContainer.RazorComponents;
@inherits NSessionComponentBase
```

The `NSessionComponentBase` class does refresh the session whenver the page is re-rendered. It also provides an instance of `NSession` with which the user can retrieve the services he registered in ```_Host.cshtml```:

```
@page "/counter"
@using BlazorSessionScopedContainer.RazorComponents;
@inherits NSessionComponentBase

<PageTitle>Counter</PageTitle>

<h1>Counter</h1>

@if (TestService != null)
{
    <p role="status">Current count: @TestService.Counter</p>
}

<button class="btn btn-primary" @onclick="IncrementCount">Click me</button>

@code {
    private TestService TestService;

    protected override void OnInitialized()
    {
        TestService = Session.GetService<TestService>();
        base.OnInitialized();
    }

    private void IncrementCount()
    {
        TestService.Counter++;
    }
}
```

The value of the counter will be kept, even if the user disconnects.


_
