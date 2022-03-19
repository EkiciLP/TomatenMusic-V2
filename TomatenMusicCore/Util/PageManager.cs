using System;
using System.Collections.Generic;


namespace TomatenMusic.Util
{
    class PageManager<T>
    {

		private List<T> Items;
		private int PageSize;

		public PageManager(List<T> allItems, int pageSize)
		{
			this.Items = allItems;
			this.PageSize = pageSize;
		}

		public List<T> GetPage(int page)
		{
			if (page <= GetTotalPages() && page > 0)
			{
				List<T> onPage = new List<T>();
				page--;

				int lowerBound = page * PageSize;
				int upperBound = Math.Min(lowerBound + PageSize, Items.Count);

				for (int i = lowerBound; i < upperBound; i++)
				{
					onPage.Add(Items[i]);
				}

				return onPage;
			}
			else
				return new List<T>();
		}

		public void AddItem(T Item)
		{
			if (Items.Contains(Item))
			{
				return;
			}
			Items.Add(Item);
		}

		public void RemoveItem(T Item)
		{

		if (Items.Contains(Item))
			Items.Remove(Item);
		}

		public int GetTotalPages()
		{
			int totalPages = (int)Math.Ceiling((double)Items.Count / PageSize);

			return totalPages;
		}

		public List<T> GetContents()
		{
			return Items;
		}
    }
}
