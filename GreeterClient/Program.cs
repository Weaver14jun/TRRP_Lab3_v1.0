// Copyright 2015 gRPC authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using Grpc.Core;
using Helloworld;
using Google.Protobuf;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace GreeterClient
{
    class Program
    {
        public static void Main(string[] args)
        {
            Channel channel = new Channel("127.0.0.1:50051", ChannelCredentials.Insecure);

            var client = new Greeter.GreeterClient(channel);
            String user = "you";

            var reply = client.SayHello(new HelloRequest { Name = user });
            var pcList = Normalizer.PCTableNormalizer.GetPCList("Data Source=bands.db;");
            JsonSerializer serializer = new JsonSerializer();
            serializer.Formatting = Formatting.Indented;
            string pcListString;
            using (StreamWriter sw = new StreamWriter("PCList.json"))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, pcList);
            }
            using (StreamReader sr = new StreamReader("PCList.json"))
            {
                pcListString = sr.ReadToEnd();
            }
            //var t = PCModel.Parser.ParseJson(pcListString);

            List<PCModel> PCModels = new List<PCModel>();
            foreach (var item in pcList)
            {
                PCModels.Add(new PCModel
                {
                    Id = item.Id,
                    Processor = item.Processor,
                    Motherboard = item.Motherboard,
                    GraphicsCard = item.GraphicsCard,
                    Ram = item.Ram,
                    Os = item.Os
                });
            }

            Msg msg = new Msg();
            msg.PCList.AddRange(PCModels);
            client.SendPCList(msg);

            //using(var output = File.Create("PCList.dat"))
            //{
            //    pcListProto.WriteTo(output);
            //}


            Console.WriteLine("Greeting: " + reply.Message);

            channel.ShutdownAsync().Wait();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
