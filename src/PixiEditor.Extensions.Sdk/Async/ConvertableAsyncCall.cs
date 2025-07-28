using PixiEditor.Extensions.CommonApi.Async;

namespace PixiEditor.Extensions.Sdk.Async;

public class ConvertableAsyncCall<TResult, TInput> : AsyncCall<TResult>
{
    public Func<TInput, TResult> Converter { get; }
    
    public ConvertableAsyncCall(Func<TInput, TResult> converter)
    {
        Converter = converter;
    }

    protected override object SetResultValue(object? result)
    {
        if(result is null)
        {
            return default(TResult);
        }
        if (result is TInput input)
        {
            return Converter(input);
        }

        throw new InvalidCastException($"Cannot convert {result.GetType().Name} to {typeof(TInput).Name}.");
    }
}
