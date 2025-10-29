using Microsoft.SemanticKernel;
using System.Text.Json;

namespace Task2_HNG13.Filters
{
    public class SqlQueryGenerator
    {
        private readonly Kernel _kernel;

        public SqlQueryGenerator(Kernel kernel)
        {
            _kernel = kernel;
        }

        public async Task<Dictionary<string, object>> GenerateSqlQueryAsync(
        string string_sorting)
        {
            var prompt = $$"""
       
        I want you to analyse the string I am going to give you. 
        Here it is: {{string_sorting}}
        It represents a way of sorting by using a property of a JSON Object.
        the properties available are here: 

        id 
        name
        capital
        region 
        population
        currency_code 
        exchange_rate 
        estimated_gdp 
        flag_url
        last_refreshed_at 
        
        The string indicates a sorting by using one of these properties in ascendant or descendent order.
        I want you to give ONLY a JSON object with two keys: "property" and "order".
        For example if the string is gdp_desc, the output should be {"property": "estimated_gdp", "order": "desc"}.
        If the order is not specified in the string, assume it is "asc".
        
       
        Again I want you to give ONLY a JSON object with two keys: {property:string, order:string}, nothing else.
        like this: {property:'id', order:'asc'}, or {property: 'name', order: 'desc'}.
        So please don't make any statement.

      
       
        """;
            try
            {
                var result = await _kernel.InvokePromptAsync(prompt);
                var json = result.ToString().Trim();

                // Parse the JSON into a dictionary
                var filters = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                return filters ?? new Dictionary<string, object>();
            }
            catch (Exception)
            {
                return new Dictionary<string, object>();
            }
        }
    }
}
