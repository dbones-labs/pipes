---
title: Example
parent: Quick Examples
has_children: false
nav_order: 3
---

# Exmaple

This is an example of how `pipes` works

Note ✏️

We reconmend that this library is used in other libraries (examples: Eventual, ArmChair etc), where you want to expose a way to extend without modification.

If you are looking for a command and query mechanism please look at a project called `MediatR`

For now we will show how this works and for you to decide if it can be helpful for you.

## 1. The program 

In our example we have created `commands` which implement an interface of `ICommand<TMessage>`

**Program execution**

This is what the simple program execution looks like, and the goal is to execte the commands in their own scopes.

```csharp
static async Task RunApp(IServiceProvider container)
{
    using var scope1 = container.CreateScope();
    var firstCommand = scope1.ServiceProvider.GetService<ICommand<Transfer>>();
    await firstCommand.Execute(new Transfer()
    {
        Amount = 10.00m,
        SourceAccountId = "a1",
        DestinationAccountId = "a2"
    });

    using var scope1 = container.CreateScope();
    var secondCommand = scope2.ServiceProvider.GetService<ICommand<ChangeAddress>>();
    await secondCommand.Execute(new ChangeAddress()
    {
        AccountId = "a1",
        NewAddress = "this is a new address"
    });
}
```

This outputs

```
Transfering 10.00, from: a1 to a2
00:00:00.0101290

updated address for a1, to this is a new address
00:00:00.0001558
```

lets observe a little before we show the setup

- we grab an instance of the command from the container, and we are aware of each scope.
- each command outputs some logic and also the time it took to execute
- we may have some other logic being applied, however we cannot see that at this stage.


## 2. Existing Command Impl

Both commands look very similar, so we will look at one and discuss

```csharp
internal class TransferCommand : ICommand<Transfer>
{
    private readonly Logger _logger;

    public TransferCommand(Logger logger)
    {
        _logger = logger;
    }

    public virtual Task Execute(Transfer transfer)
    {
        //1️⃣ measure how long the method takes
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        try
        {
            //2️⃣ validate input
            var validationContext = new ValidationContext(transfer);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(transfer, validationContext, validationResults, true))
            {
                var message = string.Join(Environment.NewLine, validationResults.Select(x => x.ErrorMessage));
                throw new ValidationException(message);
            }

            //3️⃣ do some logic here. (this is the core logic)
            _logger.Log($"Transfering {transfer.Amount}, " +
                        $"from: {transfer.SourceAccountId} " +
                        $"to {transfer.DestinationAccountId}");
        }
        catch (Exception e)
        {
            //handle logging
            _logger.Log(e);
            throw;
        }

        //4️⃣ output time taken
        stopwatch.Stop();
        _logger.Log(stopwatch.Elapsed.ToString());

        return Task.CompletedTask;
    }
}
```

We have a case of bolier-plate code.

- 1️⃣ gathering metrics
- 2️⃣ validation
- 4️⃣ exception logging

this is present in both of the commands

- the code is either identical or similar with a change of variable. 
- it is part of another requirement around software qualities, aka cross cutting concerns.

If we have many more commands this will become **increasingly in-efficient to maintain** (make change, add or remove new cross cutting concerns, this would need to be duplciated fully in all commands)

NEXT Steps

- seperate out the cross cutting concerns
- update the main program to call the pipeline

## 3 Seperation of Concerns, Introduce Pipes Actions

We have already suggested there are several concerns we can seperate out

- core logic
- metrics
- validation
- exception logging

Lets introduce Pipes to the project, in this case we will implement a number of `IAction<T>`one to represent each concern.

Note ✏️ - we want to reuse the action logic over all command types, this can be done in **several** ways. but for this we will use generics


**Cross Cutting Concern Action**


```csharp
internal class TimerAction<T> : IAction<T>
{
    private readonly Logger _logger;

    public TimerAction(Logger logger)
    {
        _logger = logger;
    }

    public async Task Execute(T context, Next<T> next)
    {
        // 1️⃣
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        // 2️⃣
        await next(context);

        // 3️⃣
        stopwatch.Stop();
        _logger.Log(stopwatch.Elapsed.ToString());
    }
}
```

- 1️⃣ Apply logic before
- 2️⃣ Execute the next action in the pipeline
- 3️⃣ Apply logic after
- `T context` is the message being passed between each action, such as `Transfer` or `ChangeAddress`

we can see that we can now seperate each of the concerns into their own actions

Here is the validation Action, just to show each action is small and does one thing.

```csharp
internal class ValidationAction<T> : IAction<T>
{
    public async Task Execute(T context, Next<T> next)
    {
        var validationContext = new ValidationContext(context);
        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(context, validationContext, validationResults, true))
        {
            var message = string.Join(Environment.NewLine, validationResults.Select(x => x.ErrorMessage));
            throw new ValidationException(message);
        }

        await next(context);
    }
}
```

In this case the logic is applied before, to next is being called at the end.

**Command Action**

In this example we want to include the command, without changing the interface, and also applying all the same cross cutting concerns.

To accmomplish this we added a wrapper `Action`, which injects the correct command for the `T context` and executes it, thats it.

```csharp
internal class CommandAction<T> : IAction<T>
{
    private readonly ICommand<T> _command;

    public CommandAction(ICommand<T> command)
    {
        _command = command;
    }
    public async Task Execute(T context, Next<T> next)
    {
        await _command.Execute(context);
    }
}
```

## 4 Wiring it up.

We need to setup

- the Middleware pipeline, this controls the order of actions being executed.
- the IoC container, which controls the lifecycle of actions.


**Middleware**

As we have implemented a Generic which can be applied against all Commands, we will implement a generic Middleware (this is so we can register easier with the IoC)

```csharp
internal class CommandMiddleware<TMessage> : IMiddleware<TMessage>
{
    private Middleware<TMessage> _middleware;

    public CommandMiddleware()
    {
        // 1️⃣
        _middleware = new ScopedMiddleware<TMessage>();

        // 2️⃣
        _middleware.Add<TimerAction<TMessage>>();
        _middleware.Add<LoggingAction<TMessage>>();
        _middleware.Add<ValidationAction<TMessage>>();
        _middleware.Add<CommandAction<TMessage>>();
    }

    public async Task Execute(IServiceProvider scope, TMessage context)
    {
        // 3️⃣
        await _middleware.Execute(scope, context);
    }
}
```

- 1️⃣ we use the scopedmiddleware, which will ensure each execute is in its own IoC scope.
- 2️⃣ the order in which we will execute the pipeline
- 3️⃣ we just call the out of the box middleware class.


**Container setup**

We use the container to control the life-cycle of all the instances. so we need to set this up

```csharp
collection.AddScoped<ICommand<Transfer>, TransferCommand>();
collection.AddScoped<ICommand<ChangeAddress>, ChangeAddressCommand>();

// 1️⃣
collection.AddScoped(typeof(LoggingAction<>));
collection.AddScoped(typeof(ValidationAction<>));
collection.AddTransient(typeof(TimerAction<>));
collection.AddScoped(typeof(CommandAction<>));

// 2️⃣
collection.AddSingleton(typeof(CommandMiddleware<>));
```

- 1️⃣ add actions (order here does not matter)
- 2️⃣ add middleware, note this class exists due to the open generic registration


## 5 App

In the app we now invoke against the middleware, and all of the actions will be executed


```csharp
static async Task RunApp(IServiceProvider container)
{
    var firstCommand = container.GetService<CommandMiddleware<Transfer>>();
    await firstCommand.Execute(container, new Transfer()
    {
        Amount = 10.00m,
        SourceAccountId = "a1",
        DestinationAccountId = "a2"
    });

    var secondCommand = container.GetService<CommandMiddleware<ChangeAddress>>();
    await secondCommand.Execute(container, new ChangeAddress()
    {
        AccountId = "a1",
        NewAddress = "this is a new address"
    });
}
```

this will output

```sh
Transfering 10.00, from: a1 to a2
00:00:00.0339117

updated address for a1, to this is a new address
00:00:00.0005452
```

## 6 Observation

- The code has a high bar of entry, the engineer will need to understand how the pattern works
- Initial setup is harder, and does not suit tiny projects.
- Code duplication is reduced
- Cross cutting concerns are easy to refactor and add into the pipeline
- The code will be fractionally slower.


we recommed to consider this where you want to offer extensibility for a reuseable library, where you do not control what people may want to add into the pipeline