namespace IvaFlashSaleEngine.Infrastructure
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RequiresIdempotencyAttribute : Attribute { }
}