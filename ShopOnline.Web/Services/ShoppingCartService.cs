using Newtonsoft.Json;
using ShopOnline.Models.Dtos;
using ShopOnline.Web.Services.Contracts;
using System.Net.Http.Json;
using System.Text;

namespace ShopOnline.Web.Services
{
    public class ShoppingCartService : IShoppingCartService
    {
        private readonly HttpClient httpClient;

        public event Action<int> OnShoppingCartChanged;

        public ShoppingCartService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }



        public async Task<CartItemDto?> AddItem(CartItemToAddDto cartItemToAddDto)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync<CartItemToAddDto>("api/ShoppingCart", cartItemToAddDto);
                if (response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                    {
                        return default(CartItemDto);
                    }
                    return await response.Content.ReadFromJsonAsync<CartItemDto>();
                }
                else
                {
                    var message = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Http status: {response.StatusCode} Message - {message}");
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<CartItemDto?> DeleteItem(int id)
        {
            try
            {
                var response = await httpClient.DeleteAsync($"api/ShoppingCart/{id}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<CartItemDto>();
                }
                return default(CartItemDto);
            }
            catch (Exception)
            {
                //Log exception
                throw;
            }
        }

        public async Task<List<CartItemDto>> GetItems(int userId)
        {
            try
            {
                var response = await httpClient.GetAsync($"api/ShoppingCart/{userId}/GetItems");
                if (response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                    {
                        return Enumerable.Empty<CartItemDto>().ToList();
                    }
                    return await response.Content.ReadFromJsonAsync<List<CartItemDto>>();
                }
                else
                {
                    var message = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Http status code: {response.StatusCode} Message: {message}");
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void RaiseEventOnShoppingCartChanged(int totalQuantity)
        {
            if (OnShoppingCartChanged != null)
            {
                OnShoppingCartChanged.Invoke(totalQuantity);
            }
        }

        public async Task<CartItemDto?> UpdateQuantity(CartItemQuantityUpdateDto cartItemQuantityUpdateDto)
        {
            try
            {
                var jsonRequest = JsonConvert.SerializeObject(cartItemQuantityUpdateDto);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json-patch+json");
                var response = await httpClient.PatchAsync($"api/ShoppingCart/{cartItemQuantityUpdateDto.CartItemId}", content);
                Console.WriteLine(response);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<CartItemDto>();
                }
                return null;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<string> GenerateBillForCart(int userId)
        {
            try
            {
                var cartItems = await GetItems(userId);
                decimal totalCost = 0;
                StringBuilder billContent = new StringBuilder();

                billContent.AppendLine("<h2>Invoice</h2>");

                foreach (var item in cartItems)
                {
                    decimal itemTotal = item.Price * item.Quantity;
                    totalCost += itemTotal;
                    billContent.AppendLine($"<p>{item.ProductName} - ${item.Price} - Quantity: {item.Quantity} - Total: ${itemTotal}</p>");
                }

                billContent.AppendLine($"<p><strong>Total: ${totalCost}</strong></p>");

                return billContent.ToString();
            }
            catch (Exception)
            {
                //Log exception
                throw;
            }
        }
    }
}
