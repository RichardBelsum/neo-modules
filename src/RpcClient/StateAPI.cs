// Copyright (C) 2015-2021 The Neo Project.
//
// The Neo.Network.RPC is free software distributed under the MIT software license,
// see the accompanying file LICENSE in the main directory of the
// project or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Neo.IO.Json;
using Neo.Network.RPC.Models;

namespace Neo.Network.RPC
{
    public class StateAPI
    {
        private readonly RpcClient rpcClient;

        public StateAPI(RpcClient rpc)
        {
            this.rpcClient = rpc;
        }

        public async Task<RpcStateRoot> GetStateRootAsync(uint index)
        {
            var result = await rpcClient.RpcSendAsync(RpcClient.GetRpcName(), index).ConfigureAwait(false);
            return RpcStateRoot.FromJson(result);
        }

        public async Task<byte[]> GetProofAsync(UInt256 rootHash, UInt160 scriptHash, byte[] key)
        {
            var result = await rpcClient.RpcSendAsync(RpcClient.GetRpcName(),
                rootHash.ToString(), scriptHash.ToString(), Convert.ToBase64String(key)).ConfigureAwait(false);
            return Convert.FromBase64String(result.AsString());
        }

        public async Task<byte[]> VerifyProofAsync(UInt256 rootHash, byte[] proofBytes)
        {
            var result = await rpcClient.RpcSendAsync(RpcClient.GetRpcName(),
                rootHash.ToString(), Convert.ToBase64String(proofBytes)).ConfigureAwait(false);

            return Convert.FromBase64String(result.AsString());
        }

        public async Task<(uint? localRootIndex, uint? validatedRootIndex)> GetStateHeightAsync()
        {
            var result = await rpcClient.RpcSendAsync(RpcClient.GetRpcName()).ConfigureAwait(false);
            var localRootIndex = ToNullableUint(result["localrootindex"]);
            var validatedRootIndex = ToNullableUint(result["validatedrootindex"]);
            return (localRootIndex, validatedRootIndex);
        }

        static uint? ToNullableUint(JObject json) => (json == null) ? (uint?)null : (uint?)json.AsNumber();

        public static JObject[] MakeFindStatesParams(UInt256 rootHash, UInt160 scriptHash, ReadOnlySpan<byte> prefix, ReadOnlySpan<byte> from = default, int? count = null)
        {
            var @params = new JObject[count.HasValue ? 5 : 4];
            @params[0] = rootHash.ToString();
            @params[1] = scriptHash.ToString();
            @params[2] = Convert.ToBase64String(prefix);
            @params[3] = Convert.ToBase64String(from);
            if (count.HasValue)
            {
                @params[4] = count.Value;
            }
            return @params;
        }

        public async Task<RpcFoundStates> FindStatesAsync(UInt256 rootHash, UInt160 scriptHash, ReadOnlyMemory<byte> prefix, ReadOnlyMemory<byte> from = default, int? count = null)
        {
            var @params = MakeFindStatesParams(rootHash, scriptHash, prefix.Span, from.Span, count);
            var result = await rpcClient.RpcSendAsync(RpcClient.GetRpcName(), @params).ConfigureAwait(false);

            return RpcFoundStates.FromJson(result);
        }

        public async Task<byte[]> GetStateAsync(UInt256 rootHash, UInt160 scriptHash, byte[] key)
        {
            var result = await rpcClient.RpcSendAsync(RpcClient.GetRpcName(),
                rootHash.ToString(), scriptHash.ToString(), Convert.ToBase64String(key)).ConfigureAwait(false);
            return Convert.FromBase64String(result.AsString());
        }
    }
}
