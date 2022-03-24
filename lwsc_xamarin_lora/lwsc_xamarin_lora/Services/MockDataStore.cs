using lwsc_xamarin_lora.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace lwsc_xamarin_lora.Services
{
    public class MockDataStore : IDataStore<Machine>
    {
        readonly List<Machine> items;

        public MockDataStore()
        {
            var status = RESTful.Query("/all_functions?username=" + App.Username + "&password=" + App.Password, RESTful.RESTType.GET, out string res, everywhere: true);
            if (status == HttpStatusCode.Unauthorized)
            {
                DependencyService.Get<IMessage>().ShortAlert("Unauthorized.");
                return;
            }
            if (status != HttpStatusCode.OK)
            {
                DependencyService.Get<IMessage>().ShortAlert("Error.");
                return;
            }

            if (res == null || res == "null")
                return;
            items = JsonConvert.DeserializeObject<Machine[]>(res).ToList();
            items = items.OrderBy(x => x.Name).ToList();
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