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

using Dapper;
using Microsoft.Data.SqlClient;
using MIS.Domain.Entities;
using MIS.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace MIS.Persistence.Repositories
{
	public class TimeItemsRepository : ITimeItemsRepository, IDisposable
	{
		private readonly IDbConnection _db;
		private readonly IDbTransaction _transaction;

		public TimeItemsRepository(String connectionString)
		{
			_db = new SqlConnection(connectionString);
			_transaction = null;
		}

		public TimeItemsRepository(IDbTransaction transaction)
		{
			_db = transaction.Connection;
			_transaction = transaction;
		}

		public async Task<List<TimeItem>> ToList(DateTime beginDate, DateTime endDate, Int32 resourceID = 0)
		{
			var result = await _db.QueryAsync<TimeItem, Resource, Doctor, Specialty, Room, VisitItem, TimeItem>(
				sql: "[dbo].[sp_TimeItems_List]",
				map: (timeItem, resource, doctor, specialty, room, visitItem) =>
				{
					timeItem.Resource = resource;
					timeItem.Resource.Doctor = doctor;
					timeItem.Resource.Doctor.Specialty = specialty;
					timeItem.Resource.Room = room;
					if (visitItem != null)
					{
						timeItem.VisitItem = visitItem;
						timeItem.VisitItem.TimeItem = timeItem;
					}

					return timeItem;
				},
				param: new { beginDate, endDate, resourceID },
				commandType: CommandType.StoredProcedure,
				transaction: _transaction
			);

			return result
				.AsList();
		}

		public async Task<List<TimeItemTotal>> GetResourceTotals(DateTime beginDate, DateTime endDate, Int32 specialtyID = 0)
		{
			var result = await _db.QueryAsync<TimeItemTotal>(
				sql: "[dbo].[sp_TimeItems_GetResourceTotals]",
				param: new { beginDate, endDate, specialtyID },
				commandType: CommandType.StoredProcedure,
				transaction: _transaction
			);

			return result
				.AsList();
		}

		public async Task<List<TimeItemTotal>> GetDispanserizationTotals(DateTime beginDate, DateTime endDate)
		{
			var result = await _db.QueryAsync<TimeItemTotal>(
				sql: "[dbo].[sp_TimeItems_GetDispanserizationTotals]",
				param: new { beginDate, endDate },
				commandType: CommandType.StoredProcedure,
				transaction: _transaction
			);

			return result
				.AsList();
		}

		public void Dispose()
		{
			if (_db != null)
			{
				_db.Dispose();
				GC.SuppressFinalize(this);
			}
		}
	}
}
