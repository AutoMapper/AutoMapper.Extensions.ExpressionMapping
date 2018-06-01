AutoMapper extentions for mapping expressions (OData)

To use, configure using the configuration helper method:

```c#
Mapper.Initialize(cfg => {
    cfg.AddExpressionMapping();
	// Rest of your configuration
});
```