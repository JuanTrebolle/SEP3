﻿using DataServer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace DataServer.Handlers
{
	class ModeratorHandler : IHandler
	{
		private TcpClient client;
		private Model model;

		private StreamWriter writer;
		private StreamReader reader;

		private bool clientConnected;

		public ModeratorHandler(TcpClient client, Model model)
		{
			this.client = client;
			this.model = model;

			NetworkStream stream = client.GetStream();
			writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };
			reader = new StreamReader(stream, Encoding.ASCII);

		}

		public void Start()
		{
			clientConnected = true;
			string request = null;

			//todo security protocol for connetion

			// Loop to receive all the data sent by the client.
			do
			{
				try
				{
					request = reader.ReadLine();
					Console.WriteLine("Received: {0}", request);

					ProcessClientRequest(request);
				}
				catch (System.IO.IOException e)
				{
					clientConnected = false;
				}

			} while (clientConnected);

			// Shutdown and end connection
			client.Close();
		}

		private void ProcessClientRequest(string request)
		{
			switch (request)
			{
				case "deletePlace":
					DeletePlace();
					break;
				case "getAllReports":
					GetAllReports();
					break;
				case "removeReview":
					break;
				case "banUser":
					break;
				case "unbanUser":
					break;
				case "authorizeUser":
					AuthorizeUser();
					break;
				default:
					Console.WriteLine("Default was called");
					break;
			}
		}
		public void DeletePlace()
		{
			long receive = long.Parse(reader.ReadLine());
			model.DeletePlace(receive);
		}

		public void GetAllReports()
		{
			writer.WriteLine(JsonSerializer.Serialize(model.GetAllPlaces()));
		}

		private void AuthorizeUser()
		{
			string receive = reader.ReadLine();
			User user = JsonSerializer.Deserialize<User>(receive);
			writer.WriteLine(model.AuthroizeUser(user));
		}
	}
}