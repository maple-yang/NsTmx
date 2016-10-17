﻿using System;
using System.IO;
using UnityEngine;
using TmxCSharp.Loader;
using TmxCSharp.Models;
using TmxCSharp.Renderer;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(TMXRenderer))]
// 简易谢谢
public class ResLoader: MonoBehaviour, ITmxLoader
{
	private TMXRenderer m_Renderer = null;
	private MeshFilter m_Filter = null;
	private Mesh m_Mesh;
	private MeshRenderer m_MeshRender = null;

	void Awake()
	{
		m_Filter = GetComponent<MeshFilter>();
		m_Renderer = GetComponent<TMXRenderer>();
		m_MeshRender = GetComponent<MeshRenderer>();
		m_Mesh = new Mesh();

		if (m_Filter != null)
			m_Filter.sharedMesh = m_Mesh;
	}

	void Start()
	{
		if (m_Renderer != null)
		{
			if (m_Renderer.LoadMapFromFile("tmx/TiledSupport-1.tmx", this))
			{
				m_Renderer.BuildAllToMesh(m_Mesh, gameObject, Camera.main);
			}
		}
	}

	public string _LoadMapXml(string fileName)
	{
		if (string.IsNullOrEmpty(fileName))
			return string.Empty;
		if (string.IsNullOrEmpty(fileName))
			return string.Empty;
		TextAsset text = Resources.Load<TextAsset>(fileName);
		if (text == null)
			return string.Empty;

		return text.text;
	}

	public Material _LoadMaterial (string fileName)
	{
		if (string.IsNullOrEmpty(fileName))
			return null;
		int idx = fileName.LastIndexOf ('.');
		if (idx >= 0) {
			fileName = fileName.Substring (0, idx);
		}
		if (string.IsNullOrEmpty(fileName))
			return null;
		Material ret = Resources.Load<Material>(fileName);
		return ret;
	}

	public void _DestroyResource(UnityEngine.Object res)
	{}
}