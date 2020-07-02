﻿/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

namespace Simulator.ScenarioEditor.UI.MapEdit
{
    using System;
    using Elements;
    using Managers;

    /// <summary>
    /// Feature allowing to remove a map element
    /// </summary>
    public class ElementMapRemove : IElementMapEdit
    {
        /// <inheritdoc/>
        public string Title { get; } = "Remove";
        
        /// <inheritdoc/>
        public Type[] TargetTypes { get; } = {typeof(ScenarioElement)};
        
        /// <inheritdoc/>
        public ScenarioElement CurrentElement { get; set; }

        /// <inheritdoc/>
        public void Edit()
        {
            ScenarioManager.Instance.SelectedElement = null;
            CurrentElement.Destroy();
        }
    }
}