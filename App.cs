﻿using RSG.Factory;
using RSG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RSG.Unity
{
    /// <summary>
    /// Singleton application class. Used AppInit to bootstrap in a Unity scene.
    /// </summary>
    public interface IApp
    {
        /// <summary>
        /// Get the factory instance.
        /// </summary>
        IFactory Factory { get; }

        /// <summary>
        /// Global logger.
        /// </summary>
        ILogger Logger { get; }
    }

    /// <summary>
    /// Singleton application class. Used AppInit to bootstrap in a Unity scene.
    /// </summary>
    public class App : IApp
    {
        /// <summary>
        /// Name of the game object automatically added to the scene that handles events on behalf of the app.
        /// </summary>
        public static readonly string AppHubObjectName = "_AppHub";

        /// <summary>
        /// Accessor for the singleton app instance.
        /// </summary>
        public static IApp Instance { get; private set; }

        public static void Init()
        {
            if (Instance != null)
            {
                // Already initialised.
                return;
            }

            Instance = new App();
        }

        public App()
        {
            var logger = new UnityLogger();
            var reflection = new Reflection();
            var factory = new Factory.Factory("App", logger, reflection);
            factory.Dep<ILogger>(logger);

            var singletonManager = InitFactory(logger, factory, reflection);

            this.Factory = factory;
            this.Logger = logger;

            singletonManager.InstantiateSingletons(factory);
            singletonManager.Startup();

            var appHub = InitAppHub();
            appHub.Shutdown = () => singletonManager.Shutdown();
        }

        /// <summary>
        /// Helper function to initalize the factory.
        /// </summary>
        private static SingletonManager InitFactory(UnityLogger logger, RSG.Factory.Factory factory, IReflection reflection)
        {           
            //todo: all this code should merge into RSG.Factory.
            factory.AutoRegisterTypes();

            var singletonManager = new SingletonManager(reflection, logger, factory);

            factory.Dep<IReflection>(reflection);
            factory.AddDependencyProvider(singletonManager);

            var singletonScanner = new SingletonScanner(reflection, logger, singletonManager);
            singletonScanner.ScanSingletonTypes();
            return singletonManager;
        }

        /// <summary>
        /// Helper function to initalize the app hub.
        /// </summary>
        private static AppHub InitAppHub()
        {
            var appHubGO = GameObject.Find(AppHubObjectName);
            if (appHubGO == null)
            {
                appHubGO = new GameObject(AppHubObjectName);
                GameObject.DontDestroyOnLoad(appHubGO);
            }

            var appHub = appHubGO.GetComponent<AppHub>();
            if (appHub == null)
            {
                appHub = appHubGO.AddComponent<AppHub>();
            }
            return appHub;
        }

        /// <summary>
        /// Get the factory instance.
        /// </summary>
        public IFactory Factory
        {
            get;
            private set;
        }

        /// <summary>
        /// Global logger.
        /// </summary>
        public ILogger Logger
        {
            get;
            private set;
        }
    }
}
