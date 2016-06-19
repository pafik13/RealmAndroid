using SDDebug = System.Diagnostics.Debug;

using System;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Android.App;
using Android.Widget;
using Android.OS;

using Realms;
using RestSharp;
using RestSharp.Serializers;

using RealmAndroid.Entities;

using suggestionscsharp;
using Android.Views;

namespace RealmAndroid
{
	[Activity(Label = "RealmAndroid", MainLauncher = true, Icon = "@mipmap/icon")]
	public class MainActivity : Activity
	{
		int count = 1;

		public SuggestClient api { get; set; }

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			Realm.DeleteRealm(RealmConfiguration.DefaultConfiguration);

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Main);

			// Get our button from the layout resource,
			// and attach an event to it
			Button button = FindViewById<Button>(Resource.Id.myButton);

			button.Click += delegate {
				button.Text = string.Format("{0} clicks!", count++);

				// realms are used to group data together
				var realm = Realm.GetInstance(); // create realm pointing to default file

				// generate test data
				var rand = new Random();
				var start = DateTime.Now;
				using (var transaction = realm.BeginWrite())
				{
					Person person;
					for (int i = 0; i < 50; i++)
					{
						person = realm.CreateObject<Person>();
						person.UUID = Guid.NewGuid().ToString();
						person.Name = String.Format("{0}_{1}", "PersonName", i);

						Dog dog;
						for (int j = 0; j < rand.Next(1, 6); j++)
						{
							dog = realm.CreateObject<Dog>();
							dog.UUID = Guid.NewGuid().ToString();
							dog.Age = i + j;
							dog.Name = "DogName" + j;
							dog.Owner = person.UUID;
						}
					}
					transaction.Commit();
				}
				var end = DateTime.Now;
				SDDebug.WriteLine($"Generate time (ms): {(end - start).TotalMilliseconds}");

				// search data
				start = DateTime.Now;
				var serchedPersons = realm.All<Person>().Where(p => p.Name == "PersonName_31");
				end = DateTime.Now;
				SDDebug.WriteLine($"Search time (ms): {(end - start).TotalMilliseconds}");
				SDDebug.WriteLine($"Persons count: {serchedPersons.Count()}");
				string personUUID = string.Empty;
				foreach (var person in serchedPersons)
				{
					SDDebug.WriteLine($"Person UUID: {person.UUID}");
					personUUID = person.UUID;
				}

				if (!string.IsNullOrEmpty(personUUID))
				{
					start = DateTime.Now;
					var serchedDogs = realm.All<Dog>().Where(d => d.Owner == personUUID);
					end = DateTime.Now;
					SDDebug.WriteLine($"Search time (ms): {(end - start).TotalMilliseconds}");
					SDDebug.WriteLine($"Dogs count: {serchedDogs.Count()}");
					foreach (var dog in serchedDogs) {
						SDDebug.WriteLine($"Dog UUID: {dog.UUID}");
					}

				}

				// upload infos
				//var client = new RestClient(@"http://demo-project-pafik13.c9users.io:8080/");
				var client = new RestClient(@"http://realm-logisapp.rhcloud.com/");

				start = DateTime.Now;
				IRestRequest requestPerson = new RestRequest(@"Person", Method.POST);
				var qPersons = realm.All<Person>();
				foreach (var itemP in qPersons)
				{
					requestPerson.Parameters.Clear();
					requestPerson.AddJsonBody(itemP);
					var responsePerson = client.Execute(requestPerson);
					if (responsePerson.StatusCode != HttpStatusCode.Created) {
						SDDebug.WriteLine($"Insert Person UNsuccess. Error:{(responsePerson.Content)}");
					}
				}
				end = DateTime.Now;
				SDDebug.WriteLine($"Persons upload time (ms): {(end - start).TotalMilliseconds}");

				start = DateTime.Now;
				IRestRequest requestDog = new RestRequest(@"Dog", Method.POST); ;
				var qDogs = realm.All<Dog>();
				foreach (var itemD in qDogs)
				{
					requestDog.Parameters.Clear();
					requestDog.AddJsonBody(itemD);
					var responseDog = client.Execute(requestDog);
					if (responseDog.StatusCode != HttpStatusCode.Created ) {
						SDDebug.WriteLine($"Insert Dog UNsuccess. Error:{(responseDog.Content)}");
					}
				}
				end = DateTime.Now;
				SDDebug.WriteLine($"Dogs upload time (ms): {(end - start).TotalMilliseconds}");
				realm.Dispose();
			};

			var token = Secret.DadataApiToken;
			var url = "https://suggestions.dadata.ru/suggestions/api/4_1/rs";
			api = new SuggestClient(token, url);
			AutoCompleteTextView text = FindViewById<AutoCompleteTextView>(Resource.Id.myAutoCompleteTextView);
			text.AfterTextChanged += (object sender, Android.Text.AfterTextChangedEventArgs e) =>
			{
				if (text.Text.Contains(" "))
				{
					var response = api.QueryAddress(text.Text);
					var suggestions = response.suggestionss.Select(x => x.value).ToArray();
					text.Adapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleDropDownItem1Line, suggestions); ;
					(text.Adapter as ArrayAdapter<String>).NotifyDataSetChanged();
					if (text.IsShown) {
						text.DismissDropDown();
					}
					text.ShowDropDown();
				}
			};
		}
	}
}


