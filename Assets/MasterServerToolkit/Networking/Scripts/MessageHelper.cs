﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MasterServerToolkit.Networking
{
    /// <summary>
    /// Helper class, that uses <see cref="IMessageFactory" /> implementation
    /// to help create messages
    /// </summary>
    public static class MessageHelper
    {
        private static IMessageFactory _factory;
        private static readonly EndianBitConverter _converter;

        static MessageHelper()
        {
            _converter = EndianBitConverter.Big;
            _factory = new MessageFactory();
        }

        /// <summary>
        /// Changes current message factory.
        /// </summary>
        /// <param name="factory"></param>
        public static void SetFactory(IMessageFactory factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Writes data into a provided packet
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="packet"></param>
        /// <returns></returns>
        public static T Deserialize<T>(byte[] data, T packet) where T : ISerializablePacket
        {
            return SerializablePacket.FromBytes(data, packet);
        }

        /// <summary>
        /// Deserializes a list of packets
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="packetCreator">Factory function</param>
        /// <returns></returns>
        public static IEnumerable<T> DeserializeList<T>(byte[] data, Func<T> packetCreator) where T : ISerializablePacket
        {
            using (var ms = new MemoryStream(data))
            {
                using (var reader = new EndianBinaryReader(EndianBitConverter.Big, ms))
                {
                    var count = reader.ReadInt32();
                    var list = new List<T>(count);

                    for (var i = 0; i < count; i++)
                    {
                        var packet = packetCreator();
                        packet.FromBinaryReader(reader);
                        list.Add(packet);
                    }

                    return list;
                }
            }
        }

        /// <summary>
        /// Creates an empty message
        /// </summary>
        /// <param name="opCode"></param>
        /// <returns></returns>
        public static IOutgoingMessage Create(ushort opCode)
        {
            return _factory.Create(opCode);
        }

        /// <summary>
        /// Creates a message with data
        /// </summary>
        /// <param name="opCode"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static IOutgoingMessage Create(ushort opCode, byte[] data)
        {
            return _factory.Create(opCode, data);
        }

        /// <summary>
        /// Creates a message from string
        /// </summary>
        /// <param name="opCode"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static IOutgoingMessage Create(ushort opCode, string message)
        {
            return _factory.Create(opCode, Encoding.UTF8.GetBytes(message));
        }

        /// <summary>
        /// Creates a message from int
        /// </summary>
        /// <param name="opCode"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IOutgoingMessage Create(ushort opCode, int value)
        {
            var bytes = new byte[4];
            _converter.CopyBytes(value, bytes, 0);
            return _factory.Create(opCode, bytes);
        }

        /// <summary>
        /// Create message from packet
        /// </summary>
        /// <param name="opCode"></param>
        /// <param name="packet"></param>
        /// <returns></returns>
        public static IOutgoingMessage Create(ushort opCode, ISerializablePacket packet)
        {
            return Create(opCode, packet.ToBytes());
        }

        /// <summary>
        /// Reconstructs message data into <see cref="IIncomingMessage" />
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="peer"></param>
        /// <returns></returns>
        public static IIncomingMessage FromBytes(byte[] buffer, int start, IPeer peer)
        {
            return _factory.FromBytes(buffer, start, peer);
        }
    }
}