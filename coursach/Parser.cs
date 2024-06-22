using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Forms;

namespace coursach
{
    public class Parser
    {
        public async Task<BindingList<Group>> ParseGroups(string url)
        {
            List<string> GroupsUrls = await GetUrls(url, "a", "div", "banner-list");
            BindingList<Group> output = new BindingList<Group>();
            List<string> ImageUrls = await GetImageUrls(url);

            foreach (string g in GroupsUrls)
            {
                output.Add(await GetGroupInfo(g, ImageUrls.FirstOrDefault()));
                ImageUrls.RemoveAt(0);
            }
            ImageUrls.Clear();
            return output;
        }

        public async Task<BindingList<Item>> ParseItems(string url, int page)
        {
            BindingList<Item> output = new BindingList<Item>();
            List<string> ItemUrls = await GetUrls(url + "?page=" + page, "a", "div", "product-preview__title");

            foreach (string s in ItemUrls)
            {
                output.Add(await GetItemInfo("https://www.starsstore.ru/" + s));
            }
            return output;
        }

        public async Task<Item> GetItemInfo(string url)
        {
            IConfiguration config = Configuration.Default.WithDefaultLoader();
            IBrowsingContext context = BrowsingContext.New(config);
            IDocument doc = await context.OpenAsync(url);

            IElement _name = doc.QuerySelector("h1");
            IElement _price = doc.QuerySelector("b.dolyame-price");
            IElement _description = doc.QuerySelector("div.content-description");

            Item item = new Item();
            if (_name != null)
                item.Name = _name.TextContent.Trim();
            else
                item.Name = "invalid name";
            if (_price != null)
            {
                int value;
                int.TryParse(string.Join("", _price.TextContent.Where(c => char.IsDigit(c))), out value);
                item.Price = (value * 4).ToString() + "₽";
            }
            else
                item.Price = "invalid price";

            if (_description != null)
                item.Description = _description.TextContent.Trim();
            else
                item.Description = "invalid description";

            return item;
        }
        
        public async Task<Group> GetGroupInfo(string url, string imgUrl)
        {
            IConfiguration config = Configuration.Default.WithDefaultLoader();
            IBrowsingContext context = BrowsingContext.New(config);
            IDocument doc = await context.OpenAsync(url);

            IElement name = doc.QuerySelector("h1");

            Group g = new Group
            {
                Name = name.TextContent.Trim(),
                Url = url,
                ImageUrl = imgUrl
            };

            return g;
        }

        public async Task<List<string>> GetImageUrls(string url)
        {
            IConfiguration config = Configuration.Default.WithDefaultLoader();
            IBrowsingContext context = BrowsingContext.New(config);
            IDocument doc = await context.OpenAsync(url);

            if (doc != null)
            {
                IEnumerable<IElement> elements = doc.All.Where(block =>
                block.LocalName == "img"
                && block.ParentElement.LocalName == "picture"
                && block.ParentElement.ParentElement.ClassList.Contains("img-ratio__inner")
                );
                List<string> urls = new List<string>();

                foreach (IElement e in elements.ToList())
                {
                    urls.Add(e.GetAttribute("src"));
                }
                return urls;
            }
            else
                return null;
        }


        public async Task<List<string>> GetUrls(string url, string lName, string parentName, string divClass)
        {
            IConfiguration config = Configuration.Default.WithDefaultLoader();
            IBrowsingContext context = BrowsingContext.New(config);
            IDocument doc = await context.OpenAsync(url);
            if (doc != null)
            {

                IEnumerable<IElement> elements = doc.All.Where(block =>
                block.LocalName == lName
                && block.ParentElement.LocalName == parentName
                && block.ParentElement.ClassList.Contains(divClass));

                List<string> output = new List<string>();
                foreach (IElement element in elements.ToList())
                {
                    output.Add(element.GetAttribute("href"));
                }
                return output;
            }
            else
                return null;
        }
    }
}
