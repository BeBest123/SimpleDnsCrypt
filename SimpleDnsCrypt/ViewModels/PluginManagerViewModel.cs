﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows.Forms;
using SimpleDnsCrypt.Config;
using Screen = Caliburn.Micro.Screen;

namespace SimpleDnsCrypt.ViewModels
{
	/// <summary>
	///     The Pluginmanager.
	/// </summary>
	[Export]
	public sealed class PluginManagerViewModel : Screen
	{
		private const string LibdcpluginLdnsIpv6 = "libdcplugin_ldns_aaaa_blocking.dll";
		private const string LibdcpluginLogging = "libdcplugin_logging.dll";
		private const string LibdcpluginCache = "libdcplugin_cache.dll";

		private bool _blockIpv6Plugin;
		private bool _logPlugin;
		private string _logPluginPath;
		private bool _cachePlugin;
		private int _cachePluginTtl;
		private List<string> _plugins;

		/// <summary>
		///     PluginManagerViewModel constructor.
		/// </summary>
		[ImportingConstructor]
		public PluginManagerViewModel()
		{
			_plugins = new List<string>();
			_cachePluginTtl = 60;
		}

		/// <summary>
		///     List of plugins.
		/// </summary>
		public List<string> Plugins
		{
			get { return _plugins; }
			set
			{
				_plugins = value;
				NotifyOfPropertyChange(() => Plugins);
			}
		}

		/// <summary>
		///     To manage the block IPv6 plugin.
		/// </summary>
		public bool BlockIpv6Plugin
		{
			get { return _blockIpv6Plugin; }
			set
			{
				_blockIpv6Plugin = value;
				if (value)
				{
					Plugins.Add(LibdcpluginLdnsIpv6);
				}
				else
				{
					Plugins.Remove(LibdcpluginLdnsIpv6);
				}
				NotifyOfPropertyChange(() => BlockIpv6Plugin);
			}
		}

		/// <summary>
		///     To manage the cache plugin.
		/// </summary>
		public bool CachePlugin
		{
			get { return _cachePlugin; }
			set
			{
				_cachePlugin = value;
				if (value)
				{
					foreach (var plugin in Plugins)
					{
						if (plugin.StartsWith(LibdcpluginCache))
						{
							Plugins.Remove(plugin);
						}
					}
					Plugins.Add(LibdcpluginCache + ",--min-ttl=" + CachePluginTtl);
				}
				else
				{
					foreach (var plugin in Plugins)
					{
						if (plugin.StartsWith(LibdcpluginCache))
						{
							Plugins.Remove(plugin);
							NotifyOfPropertyChange(() => CachePlugin);
						}
					}
				}
				NotifyOfPropertyChange(() => CachePlugin);
			}
		}

		/// <summary>
		///     To hold the cache TTL.
		/// </summary>
		public int CachePluginTtl
		{
			get { return _cachePluginTtl; }
			set
			{
				_cachePluginTtl = value;
				NotifyOfPropertyChange(() => CachePluginTtl);
			}
		}

		/// <summary>
		///     To manage the log plugin.
		/// </summary>
		public bool LogPlugin
		{
			get { return _logPlugin; }
			set
			{
				_logPlugin = value;
				if (value)
				{
					if (LogPluginPath != null && Directory.Exists(Path.GetDirectoryName(LogPluginPath)))
					{
						Plugins.Add(LibdcpluginLogging + "," + LogPluginPath);
						_logPlugin = true;
					}
					else
					{
						_logPlugin = false;
					}
				}
				else
				{
					foreach (var plugin in Plugins)
					{
						if (plugin.StartsWith(LibdcpluginLogging))
						{
							Plugins.Remove(plugin);
							NotifyOfPropertyChange(() => LogPlugin);
						}
					}
				}
				NotifyOfPropertyChange(() => LogPlugin);
			}
		}

		/// <summary>
		///     The full path to the log file.
		/// </summary>
		public string LogPluginPath
		{
			get { return _logPluginPath; }
			set
			{
				_logPluginPath = value;
				NotifyOfPropertyChange(() => LogPluginPath);
			}
		}

		/// <summary>
		///     FolderBrowserDialog to select the log folder.
		/// </summary>
		public void SelectFolder()
		{
			try
			{
				var dialog = new FolderBrowserDialog();
				var result = dialog.ShowDialog();
				if (result != DialogResult.OK) return;
				LogPluginPath = Path.Combine(dialog.SelectedPath, Global.DefaultLogFileName);
			}
			catch (Exception)
			{
				LogPluginPath = string.Empty;
			}
		}

		/// <summary>
		///     Set the used plugins.
		/// </summary>
		/// <param name="plugins">List of plugins.</param>
		public void SetPlugins(List<string> plugins)
		{
			_plugins = plugins;
			foreach (var plugin in _plugins)
			{
				if (plugin.Equals(LibdcpluginLdnsIpv6))
				{
					_blockIpv6Plugin = true;
				}
				if (plugin.StartsWith(LibdcpluginLogging))
				{
					var a = plugin.Split(',');
					_logPluginPath = a[1];
					_logPlugin = true;
				}
				if (plugin.StartsWith(LibdcpluginCache))
				{
					var a = plugin.Split(',');
					if (a[1].StartsWith("--min-ttl"))
					{
						var b = a[1].Split('=');
						_cachePluginTtl = Convert.ToInt32(b[1]);
					}
					else
					{
						_cachePluginTtl = 60;
					}
					_cachePlugin = true;
				}
			}
		}

		/// <summary>
		///     Close the dialog with a positiv result.
		/// </summary>
		public void SendOk()
		{
			TryClose(true);
		}
	}
}