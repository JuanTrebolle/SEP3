﻿using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Client.Models;
using System.Threading;
using System.Text.Json;

namespace Client.Data
{
    public class Map : IMap
    {
        private readonly IJSRuntime jsRuntime;
        private readonly IModel model;
        private DotNetObjectReference<Map> objRef;

        private bool addingMarkerMode;
        private bool markerAdded;

        private double currentLongitude = 0;
        private double currentLatitude = 0;

        private static bool dataReady = false;

        public Map(IJSRuntime jsRuntime, IModel model)
        {
            this.jsRuntime = jsRuntime;
            this.model = model;
            addingMarkerMode = false;
            markerAdded = false;

            model.OnNewPlace -= AddMarker;
            model.OnMapLoaded -= DataReady;
            model.OnMapLoaded += DataReady;
        }

        public async Task InitMapAsync()
        {
            if (objRef == null)
                objRef = DotNetObjectReference.Create(this);
            await jsRuntime.InvokeVoidAsync("mapBoxFunctions.initMapBox", objRef);

            while (!dataReady)
                await Task.Delay(100);

            model.OnNewPlace -= AddMarker;
            model.OnNewPlace += AddMarker;

            foreach (Place place in model.GetPlaces())
            {
                await AddMarkerAsync(place);
            }
        }

        public async Task InitMapMarkerAsync(long id)
        {
            if (objRef == null)
                objRef = DotNetObjectReference.Create(this);
            model.OnNewPlace -= AddMarker;
            await jsRuntime.InvokeVoidAsync("mapBoxFunctions.initMapBox", objRef);
            await AddMarkerAsync(model.GetPlaceById(id));

        }

        public async Task InitSavedPlacesAsync(List<Place> places)
        {
            if (objRef == null)
                objRef = DotNetObjectReference.Create(this);
            model.OnNewPlace -= AddMarker;
            await jsRuntime.InvokeVoidAsync("mapBoxFunctions.initMapBox", objRef);

            
            foreach (Place place in places)
                await AddMarkerAsync(place);
            
        }

        public void DataReady()
        {
            dataReady = true;
        }

        public async Task AddMarkerAsync(Place place)
        {
            await jsRuntime.InvokeVoidAsync("mapBoxFunctions.addMarker", place.longitude, place.latitude, place.title, place.description, place.id);
        }

        public void AddMarker(Place place)
        {
            jsRuntime.InvokeVoidAsync("mapBoxFunctions.addMarker", place.longitude, place.latitude, place.title, place.description, place.id);

        }

        public bool GetAddingMarkerMode()
        {
            return addingMarkerMode;
        }

        public void ChangeAddingMarkerMode()
        {
            addingMarkerMode = !addingMarkerMode;
            markerAdded = false;
        }

        public async Task SetTemporaryMarkerAsync()
        {
            await jsRuntime.InvokeVoidAsync("mapBoxFunctions.setTemporaryMarker", currentLongitude, currentLatitude);
            markerAdded = true;
        }

        public async Task RemoveTemporaryMarkerAsync()
        {
            await jsRuntime.InvokeVoidAsync("mapBoxFunctions.removeTemporaryMarker");
        }


        public async Task CreatePlace(PlaceData placeData)
        {
            
            Place newPlace = new Place() {
                longitude = currentLongitude,
                latitude = currentLatitude,
                title = placeData.Title,
                description = placeData.Description,
                addedBy = placeData.addedBy,
                reviews = new List<Review>()
            };
            //await AddMarkerAsync(newPlace); //line will be removed and place will be added when model gets it from broadcaster


            if (markerAdded)
            {
                await RemoveTemporaryMarkerAsync();
                await model.AddPlaceAsync(newPlace);
            }
            else
            {
                throw new Exception("You forgot to choose a location!");
            }
        }

        [JSInvokable("MapClickAsync")]
        public async Task MapClickedAsync(double longitude, double latitude)
        {
            currentLatitude = latitude;
            currentLongitude = longitude;

            if (addingMarkerMode)
            {
                await SetTemporaryMarkerAsync();
            }
        }

        [JSInvokable("UpdateCoordinates")]
        public void UpdateCoordinates(double longitude, double latitude)
        {
            currentLatitude = latitude;
            currentLongitude = longitude;
        }

        [JSInvokable("ReportPlace")]
        public async Task ReportPlace(long id)
        {
            Console.WriteLine(id);
            await model.ReportPlaceAsync(id);
        }

        [JSInvokable("GetPlaceDetails")]
        public async Task GetPlaceDetails(long id)
        {
            Console.WriteLine(id);
            Place place = model.GetPlaces().FirstOrDefault(p => p.id.Equals(id));
            await jsRuntime.InvokeVoidAsync("mapBoxFunctions.addReviewLite", place.GetRating());
        }
    }
}
