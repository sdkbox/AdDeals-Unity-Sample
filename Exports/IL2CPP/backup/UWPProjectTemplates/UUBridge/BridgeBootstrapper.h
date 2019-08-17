#pragma once

namespace UUBridge
{
	public interface class IUWPBridge
	{
	public:
		void Send(Platform::String^ json);
	};

	public interface class IUnityBridge
	{
	public:
		void Send(Platform::String^ json);
	};

	public ref class BridgeBootstrapper sealed
	{
	public:
		static IUWPBridge^ GetUWPBridge()
		{
			return m_UWPBridge;
		}

		static void SetUWPBridge(IUWPBridge^ bridge)
		{
			m_UWPBridge = bridge;
		}

		static IUnityBridge^ GetUnityBridge()
		{
			return m_UnityBridge;
		}

		static void SetUnityBridge(IUnityBridge^ bridge)
		{
			m_UnityBridge = bridge;
		}

	private:
		static IUWPBridge^ m_UWPBridge;
		static IUnityBridge^ m_UnityBridge;

		BridgeBootstrapper();
	};

	IUWPBridge^ BridgeBootstrapper::m_UWPBridge;
	IUnityBridge^ BridgeBootstrapper::m_UnityBridge;
}

