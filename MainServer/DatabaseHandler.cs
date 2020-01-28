using System;
using System.Collections.Generic;
using System.Text;
using SharedStuff;

namespace MainServer {
    class DatabaseHandler {
        public DatabaseHandler() {
            SocketServer.RegisterHandler(MessageType.Sensordata, SensorDataHandler);
            SocketServer.RegisterHandler(MessageType.Image, ImageHandler);
        }

        public void SensorDataHandler(byte[] bytes) {
            
        }
        public void ImageHandler(byte[] bytes) {
            Console.Out.WriteLine("Image thing");
        }
    }
}
