﻿#region Copyright © 2020-2021 Vladimir Deryagin. All rights reserved
/*
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
#endregion

using MIS.Demo.DataContexts;
using MIS.Domain.Entities;
using MIS.Domain.Providers;
using MIS.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace MIS.Demo.Repositories
{
	public class VisitItemsRepository : IVisitItemsRepository
	{
		private readonly DemoDataContext _dataContext;

		public VisitItemsRepository(
			IDateTimeProvider _,
			DemoDataContext dataContext
		)
		{
			_dataContext = dataContext;
		}

		public async Task<Int32> Create(VisitItem item)
		{
			if (_dataContext.VisitItems.FirstOrDefault(vi => vi.TimeItemID == item.TimeItemID) != null)
			{
				throw new Exception("Visit item already exists!");
			}

			item.TimeItem = _dataContext.TimeItems.FirstOrDefault(ti => ti.ID == item.TimeItemID);
			item.TimeItem.VisitItem = item;

			item.Patient = _dataContext.Patients.FirstOrDefault(p => p.ID == item.PatientID);

			item.ID = _dataContext.VisitItems.Count > 0 ? _dataContext.VisitItems.Max(vi => vi.ID) + 1 : 1;

			_dataContext.VisitItems.Add(item);

			var result = item.ID;

			return await Task.FromResult(result);
		}

		public async Task<VisitItem> Get(Int32 visitItemID)
		{
			var result = _dataContext.VisitItems
				.FirstOrDefault(vi => vi.ID == visitItemID);

			return await Task.FromResult(result);
		}

		public async Task<List<VisitItem>> ToList(DateTime beginDate, DateTime endDate, Int32 patientID = 0)
		{
			var result = _dataContext.VisitItems
				.Where(vi => vi.TimeItem.Resource.Doctor.Specialty.ID > 0)
				.Where(vi => vi.TimeItem.Date >= beginDate && vi.TimeItem.Date <= endDate && (patientID == 0 || vi.PatientID == patientID))
				.ToList();

			return await Task.FromResult(result);
		}
	}
}
