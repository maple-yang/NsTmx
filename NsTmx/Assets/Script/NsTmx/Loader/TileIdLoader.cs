﻿using System;
using System.Collections.Generic;
using System.IO;
using TmxCSharp.Models;
using XmlParser;
using UnityEngine;

namespace TmxCSharp.Loader
{
    internal class TileIdLoader
    {
        private readonly TileMapSize _size;
        private readonly int _expectedIds;
        
        public TileIdLoader(TileMapSize tileMapSize)
        {
            if (tileMapSize == null)
            {
				#if DEBUG
				Debug.LogError("tileMapSize is null");
				#endif
                return;
            }

            _size = tileMapSize;
            _expectedIds = tileMapSize.Width * tileMapSize.Height;
        }

        public void LoadLayer(MapLayer mapLayer, XMLNode layerData)
        {
            if (mapLayer == null)
            {
                throw new ArgumentNullException("mapLayer");
            }

            if (layerData == null)
            {
                throw new System.Exception("Layer does not have a data element");
            }

            string encoding = layerData.GetValue("@encoding");

            switch (encoding)
            {
                case "base64":
                    string dataStr = layerData.GetValue("_text");

                    ApplyIds(GetMapIdsFromBase64(dataStr, layerData.GetValue("@compression")), mapLayer);
                    break;

                case "csv":
                    ApplyIds(ParseCsvData(layerData), mapLayer);
                    break;

                default:
                    if (string.IsNullOrEmpty(encoding))
                    {
                        XMLNodeList tileList = layerData.GetNodeList("tile");
                        ApplyIds(GetMapIdsFromXml(tileList), mapLayer);
                    }
                    else
                    {
					#if DEBUG
					Debug.LogError("Unsupported layer data encoding (expected base64 or csv)");
                    #endif
					}
                    break;
            }
        }

        private IList<int> GetMapIdsFromBase64(string value, string compression)
        {
            return GetMapIdsFromBytes(Decompression.Decompress(compression, Convert.FromBase64String(value)));
        }

        private IList<int> GetMapIdsFromXml(XMLNodeList tiles)
        {
            IList<int> ret = null;

            for (int i = 0; i < tiles.Count; ++i)
            {
                XMLNode tile = tiles[i] as XMLNode;
                if (tile == null)
                    continue;
                string s = tile.GetValue("@gid");
                if (string.IsNullOrEmpty(s))
                    continue;
                int gid;
                if (!int.TryParse(s, out gid))
                    continue;
                if (ret == null)
                    ret = new List<int>();
                ret.Add(gid);
            }

            return ret;
        }

        private void ApplyIds(IList<int> ids, MapLayer layer)
        {
            IEnumerator<int> enumerator = ids.GetEnumerator();

            for (int y = 0; y < _size.Height; y++)
            {
                for (int x = 0; x < _size.Width; x++)
                {
                    enumerator.MoveNext();

                    layer.TileIds[y, x] = enumerator.Current;
                }
            }

            enumerator.Dispose();
        }

        private IList<int> ParseCsvData(XMLNode layerData)
        {
            if (layerData == null)
                return null;

            string str = layerData.GetValue("_text");
            if (string.IsNullOrEmpty(str))
                return null;

            string[] ss = str.Split(new char[] {','});
            if (ss == null || ss.Length <= 0)
                return null;

            IList<int> ret = null;
            for (int i = 0; i < ss.Length; ++i)
            {
                string s = ss[i];
                if (string.IsNullOrEmpty(s))
                    continue;
                int idx;
                if (int.TryParse(s, out idx))
                {
                    if (ret == null)
                        ret = new List<int>();
                    ret.Add(idx);
                }
            }

            return ret;
        }

        private IList<int> GetMapIdsFromBytes(byte[] decompressedData)
        {
            int expectedBytes = _expectedIds * 4;

            if (decompressedData.Length != expectedBytes)
            {
				#if DEBUG
				Debug.LogError("Decompressed data is not identical in size to map");
                #endif
				return null;
            }

            IList<int> ret = null;
            for (int tileIndex = 0; tileIndex < expectedBytes; tileIndex += 4)
            {
                int tileId = GetTileId(decompressedData, tileIndex);
                if (ret == null)
                    ret = new List<int>();
                ret.Add(tileId);
            }

            return ret;
        }

        private static int GetTileId(byte[] decompressedData, int tileIndex)
        {
            const uint flippedHorizontallyFlag = 0x80000000;
            const uint flippedVerticallyFlag = 0x40000000;
            const uint flippedDiagonallyFlag = 0x20000000;
            const uint flipMask = ~(flippedHorizontallyFlag | flippedVerticallyFlag | flippedDiagonallyFlag);

            long tileId = decompressedData[tileIndex]
                          | (decompressedData[tileIndex + 1] << 8)
                          | (decompressedData[tileIndex + 2] << 16)
                          | (decompressedData[tileIndex + 3] << 24);

            // TODO: support these flags

            bool flippedHorizontally = (tileId & flippedHorizontallyFlag) > 0;
            bool flippedVertically = (tileId & flippedVerticallyFlag) > 0;
            bool flippedDiagonally = (tileId & flippedDiagonallyFlag) > 0;

            return (int)(tileId & flipMask);
        }
    }
}