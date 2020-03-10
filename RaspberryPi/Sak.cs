using System;
using System.Collections.Generic;
using System.Text;
using System.Device.Gpio;
using SharedStuff;

namespace RaspberryPi {

    class Sak {
        private static Sak[] saker;

        public static void InitalizeSaker() {
            Console.WriteLine("Epic");
            saker = new Sak[] {
                new Vattensak(new []{"MoistureSensor1"}, 69.0f, 5),
                new Lampsak(new []{"Lampsensor1", "Lampsensor2" }, 43.02f, 43)
            };
        }
        public static void UpdateSaker() {
            foreach(Sak sak in saker) {
                sak.update();
            }
        }

        protected static List<T> GetSensorList<T>(string[] sensorNames) where T : Sensor {
            List<T> sensorList = new List<T>();

            foreach (var sName in sensorNames) {
                foreach (var sensor in SensorList.Sensors) {
                    if (sensor.SensorName == sName) {
                        sensorList.Add((T)sensor);
                    }
                }
            }

            return sensorList;
        }

        protected virtual void update() { }
    }

    class Vattensak : Sak {
        private List<MoistureSensor> m_moistureSensors;
        private float m_maxSensorValue;
        GpioController m_gpioController;
        int m_pin;
        public Vattensak(string[] sensorNames, float maxSensorValue, int pin) {
            m_moistureSensors = GetSensorList<MoistureSensor>(sensorNames);
            m_gpioController = new GpioController();
            m_pin = pin;
            m_gpioController.OpenPin(pin);
            m_maxSensorValue = maxSensorValue;
        }

        protected override void update() {
            float moisture = 0.0f;
            foreach(var mSensor in m_moistureSensors) {
                moisture += mSensor.Moisture;
            }

            moisture /= (float)m_moistureSensors.Count;

            if(moisture < m_maxSensorValue) {
                m_gpioController.Write(m_pin, PinValue.High);
            }
            else {
                m_gpioController.Write(m_pin, PinValue.Low);
            }
        }

    }

    class Lampsak : Sak {
        private List<LightSensor> m_lightSensors;
        private float m_maxSensorValue;
        GpioController m_gpioController;
        int m_pin;

        public Lampsak(string[] sensorNames, float maxSensorValue, int pin) {
            m_lightSensors = GetSensorList<LightSensor>(sensorNames);
            m_gpioController = new GpioController();
            m_pin = pin;
            m_gpioController.OpenPin(pin);
            m_maxSensorValue = maxSensorValue;
        }

        protected override void update() {
            float light = 0.0f;
            foreach (var mSensor in m_lightSensors) {
                light += mSensor.Light;
            }

            light /= (float)m_lightSensors.Count;

            if (light < m_maxSensorValue) {
                m_gpioController.Write(m_pin, PinValue.High);
            }
            else {
                m_gpioController.Write(m_pin, PinValue.Low);
            }
        }
    }
}
