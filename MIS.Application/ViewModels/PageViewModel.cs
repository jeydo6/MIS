﻿using System;
using System.Collections.Generic;

namespace MIS.Application.ViewModels
{
	public class PageViewModel
	{
		public PageViewModel()
		{
			Objects = new List<Object>();
		}

		public ICollection<Object> Objects { get; set; }
	}
}
