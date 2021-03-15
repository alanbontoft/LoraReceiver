using System;
using System.IO.Ports;
using System.Device.Gpio;

namespace loraserver
{
    class Program
    {
        const int GPIO0 = 17;
        const int GPIO1 = 18;
        static void Main(string[] args)
        {
            SerialPort port = null;
            
            Console.WriteLine("LoRa Receiver\nWaiting for data...");

            try
            {
                var controller = new GpioController();

                controller.OpenPin(17, PinMode.Output);
                controller.OpenPin(18, PinMode.Output);
                controller.Write(GPIO0, PinValue.High);
                controller.Write(GPIO1, PinValue.High);

                var commands = new byte[] { 0xC1, 0xC1, 0xC1 };

                var configure = new byte[] { 0xC2, 0x00, 0x01, 0x1A, 0x17, 0x44 };
                var message = string.Empty;

                var rxbuffer = new byte[50];
                var portName = "/dev/serial0";

                port = new SerialPort()
                {
                    PortName = portName,
                    BaudRate = 9600,
                    DataBits = 8,
                    Parity = Parity.None,
                    StopBits = StopBits.One
                };

                port.Open();

                // write config
                port.Write(configure, 0, configure.Length);

                System.Threading.Thread.Sleep(10);

                // put into normal mode
                controller.Write(GPIO0, PinValue.Low);
                controller.Write(GPIO1, PinValue.Low);

                System.Threading.Thread.Sleep(50);

                // clear rx buffer
                port.DiscardInBuffer();

                while (true)
                {
                    // check bytes available
                    int bytesReady = port.BytesToRead;

                    if (bytesReady > 0)
                    {
                        // limit chars read to buffer size
                        var bytes = (bytesReady > rxbuffer.Length) ? rxbuffer.Length : bytesReady;
                        
                        port.Read(rxbuffer, 0, bytes);

                        for (int i=0; i < bytes; i++)
                        {
                            // add char to message or write to screen if terminator and reset string
                            if (rxbuffer[i] != '\r')
                            {
                                message += (char)rxbuffer[i];
                            }
                            else
                            {
                                Console.WriteLine($"{DateTime.Now} : {message}");
                                message = string.Empty;
                            }
                        }
                    }
                    
                    System.Threading.Thread.Sleep(100);
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            if (port != null) port.Close();
        }
    }
}
