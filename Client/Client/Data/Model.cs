﻿using Client.Data.Networking;
using Client.Models;
using Client.Networking;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Client.Data
{
	public class Model : IModel
	{
		private IList<Place> places;
		private readonly ClientImp server;

		public Model()
		{
			server = new ClientImp();
			var loaderThread = new Thread(LoadPlaces);
			loaderThread.Name = "Init Place Loader";
			loaderThread.Start();
			server.listener.OnNewPlace += ReceivePlace;
			server.listener.OnUpdatePlace += UpdatePlace;
		}

		private void LoadPlaces()
		{
			places = server.GetPlacesAsync().Result;
			OnMapLoaded?.Invoke();
            System.Console.WriteLine(places.FirstOrDefault().reviews.GetRating());
		}

		public override async Task AddPlaceAsync(Place place)
		{
			await server.AddPlaceAsync(place);
			places.Add(place);
		}

		public override IList<Place> GetPlaces()
		{
			return places;
		}

		private void ReceivePlace(Place place)
		{
			places.Add(place);
			OnNewPlace?.Invoke(place);
		}

		public override async Task<List<Report<Place>>> GetPlaceReportsAsync()
		{
			return await server.GetPlaceReportsAsync();
		}

		public override Task RemovePlaceAsync(long placeId)
		{
			throw new System.NotImplementedException();
		}

		public override Task DismissPlaceReportAsync(long reportId)
		{
			throw new System.NotImplementedException();
		}
		
		public override async Task ReportPlaceAsync(long id)
		{
			Place place = GetPlaces().FirstOrDefault(p => p.id.Equals(id));
			Report<Place> report = new Report<Place>
			{
				reportedItem = place,
				reportedClass = "Place"
			};
			await server.ReportPlaceAsync(report);
		}

		public override Place GetPlaceById(long id)
        {
			return GetPlaces().FirstOrDefault(p => p.id.Equals(id));
		}

		public override async Task AddPlaceRatingAsync(long placeId, int r)
		{
			ReviewItem review = new ReviewItem() {
				rating = r
			};
			await server.AddPlaceReviewAsync(placeId, review);
		}

		private void UpdatePlace(Place place)
		{
			places.Remove(GetPlaceById(place.id));
			places.Add(place);
		}
	}
}
