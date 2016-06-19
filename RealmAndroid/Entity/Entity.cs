using Realms;
using RestSharp.Serializers;


namespace RealmAndroid.Entities
{
	// Define your models like regular C# classes
	public class Dog : RealmObject
	{
		[ObjectId]
		public string UUID { get; set; }
		[Indexed]
		public string Owner { get; set; }
		[Ignored]
		public double? Latitude { get; set; }

		public string Name { get; set; }

		public int Age { get; set; }
	}

	public class Person : RealmObject
	{
		[ObjectId]
		public string UUID { get; set; }

		public string Name { get; set; }
	}

}

