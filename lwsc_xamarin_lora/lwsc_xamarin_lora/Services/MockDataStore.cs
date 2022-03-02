using lwsc_xamarin_lora.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace lwsc_xamarin_lora.Services
{
    public class MockDataStore : IDataStore<Machine>
    {
        readonly List<Machine> items;

        public MockDataStore()
        {
            var status = RESTful.Query("/all_functions", RESTful.RESTType.GET, out string res);

            if (res == null || res == "null")
                return;
            items = JsonConvert.DeserializeObject<Machine[]>(res).ToList();
        }

        public async Task<bool> AddItemAsync(Machine item)
        {
            items.Add(item);

            return await Task.FromResult(true);
        }

        public async Task<bool> UpdateItemAsync(Machine item)
        {
            var oldItem = items.Where((Machine arg) => arg.Name == item.Name).FirstOrDefault();
            items.Remove(oldItem);
            items.Add(item);

            return await Task.FromResult(true);
        }

        public async Task<bool> DeleteItemAsync(string name)
        {
            var oldItem = items.Where((Machine arg) => arg.Name == name).FirstOrDefault();
            items.Remove(oldItem);

            return await Task.FromResult(true);
        }

        public async Task<Machine> GetItemAsync(string name)
        {
            return await Task.FromResult(items.FirstOrDefault(s => s.Name == name));
        }

        public async Task<IEnumerable<Machine>> GetItemsAsync(bool forceRefresh = false)
        {
            return await Task.FromResult(items);
        }
    }
}