using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MoviesAPI.Utilities
{
    public class TypeBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var propertyName = bindingContext.ModelName;
            var value = bindingContext.ValueProvider.GetValue(propertyName);

            if(value == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            try
            {
                var destinationType = bindingContext.ModelMetadata.ModelType;
                var deserializedValue = System.Text.Json.JsonSerializer.Deserialize(value.FirstValue!, 
                    destinationType, new System.Text.Json.JsonSerializerOptions {  PropertyNameCaseInsensitive = true });
                bindingContext.Result = ModelBindingResult.Success(deserializedValue);
            }
            catch
            {
                bindingContext.ModelState.TryAddModelError(propertyName, "The value was not of the appropriate data type");
            }

            return Task.CompletedTask;
        }
    }
}
