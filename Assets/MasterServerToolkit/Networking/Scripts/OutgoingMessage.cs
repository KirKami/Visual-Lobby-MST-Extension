﻿using System;

namespace MasterServerToolkit.Networking
{
    /// <summary>
    /// Represents an outgoing message
    /// </summary>
    public class OutgoingMessage : IOutgoingMessage
    {
        public OutgoingMessage(ushort opCode) : this(opCode, new byte[0])
        {
            OpCode = opCode;
            Status = 0;
        }

        public OutgoingMessage(ushort opCode, byte[] data)
        {
            OpCode = opCode;
            Status = 0;
            SetBinary(data);
        }

        /// <summary>
        /// 
        /// </summary>
        public int? ReceiverId { get; set; }

        /// <summary>
        /// Operation code, a.k.a message type
        /// </summary>
        public ushort OpCode { get; private set; }

        /// <summary>
        /// Content of the message
        /// </summary>
        public byte[] Data { get; private set; }

        /// <summary>
        /// Returns true if data is not empty
        /// </summary>
        public bool HasData
        {
            get { return Data != null && Data.Length > 0; }
        }

        /// <summary>
        /// An id of ack request. It's set when we send a message,
        /// and expect a response. This is how we tell which message we got a response to
        /// </summary>
        public int? AckRequestId { get; set; }

        /// <summary>
        /// Used to identify what message we are responsing to
        /// </summary>
        public int? AckResponseId { get; set; }

        /// <summary>
        /// Internal flags, used to help identify what kind of message we've received
        /// </summary>
        public byte Flags { get; set; }

        /// <summary>
        /// Status code of the message
        /// </summary>
        public ResponseStatus Status { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public IOutgoingMessage SetBinary(byte[] data)
        {
            Data = data;
            return this;
        }

        /// <summary>
        /// Serializes message to byte array
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            var converter = EndianBitConverter.Big;
            var flags = GenerateFlags(this);

            var dataLength = Data.Length;
            var isAckRequest = (flags & (byte)MessageFlag.AckRequest) > 0;
            var isAckResponse = (flags & (byte)MessageFlag.AckResponse) > 0;

            var packetSize = 1 // Flags
                             + 2 // OpCode
                             + 4 // Data length number
                             + dataLength // Data
                             + (isAckRequest ? 4 : 0) // Ack Request id
                             + (isAckResponse ? 5 : 0); // Ack Response id (int + byte);

            var messagePacket = new byte[packetSize];

            var pointer = 0;
            messagePacket[0] = flags;
            pointer++; // Write Flags
            converter.CopyBytes(OpCode, messagePacket, pointer);
            pointer += 2; // Write OpCode
            converter.CopyBytes(dataLength, messagePacket, pointer);
            pointer += 4; // Data Length
            Array.Copy(Data, 0, messagePacket, pointer, dataLength);
            pointer += dataLength; // Data

            if (isAckRequest)
            {
                converter.CopyBytes(AckRequestId.Value, messagePacket, pointer);
                pointer += 4;
            }

            if (isAckResponse)
            {
                converter.CopyBytes(AckResponseId.Value, messagePacket, pointer);
                pointer += 4;

                // Status code
                messagePacket[pointer] = (byte)Status;
            }

            return messagePacket;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static byte GenerateFlags(IOutgoingMessage message)
        {
            var flags = message.Flags;

            if (message.AckRequestId.HasValue)
            {
                flags |= (byte)MessageFlag.AckRequest;
            }

            if (message.AckResponseId.HasValue)
            {
                flags |= (byte)MessageFlag.AckResponse;
            }

            return flags;
        }
    }
}