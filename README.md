# chain

Chain is a pattern that I found useful in my places in my code. It is NOT the chain-of-responsibility and it is not the work-flow 
or pipeline. It is a combination of all.

Chain contains of a manager class that implements `IChain<T>` plus as many link classes which they should implement `ILink<T>`. 
'IChain' have a methid called 'ExecuteAll()' which technically 
goes thourgh the `Execute()` method of all links and run them one by one. The manager class is responsible to define the sequence
of the links. 

Manager class also can have a method called `ClosingAction`. This action will be run after execution of all links. 

Each links has two different abilities. They can stop execution of the chain, and they can stop propagation of the chain and jump to
ClosingAction. The difference between two is that ClosingAction method, if exists, won't be run for StopExecution() but it will for 
StopPropagation().

There is a ready to use chain called `EmptyChain` in this assembly. New chains can be implemented either directly from `IChain` or by 
inheriting from `EmptyChain`. 
``` csharp
var message = new Message { Name = "M1" };

var c = new EmptyChain<Message>();
c.AddLink<L1>();
c.AddLink<L2>();

c.ExecuteAll(message);
```

`EmptyChain` can also be used in a different way:
``` csharp
public class L1L2Chain : EmptyChain<Message>
{
    public L1L2Chain()
    {
        AddLink<L1>();
        AddLink<L2>();
    }
}

var c = new L1L2Chain();
c.ExecuteAll(message);
```

Chains can also be created in IoC registration. This gives the flexibility to define the sequence of the chain in the IoC. 
``` csharp
_container = new Container(_ => _.For<IChain<Message>>().Use(CreateChain()).Singleton());

private static IChain<Message> CreateChain()
{
    var chain = new EmptyChain<Message>();
    chain.AddLink<L1>();
    chain.AddLink<L2>();
    return chain;
}
```

