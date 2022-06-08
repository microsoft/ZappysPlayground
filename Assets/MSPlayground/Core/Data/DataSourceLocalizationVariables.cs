
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Data;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

namespace MSPlayground.Core.Data
{
    /// <summary>
    /// A data source to integrate {UnityEngine.Localization.SmartFormat.PersistentVariables} (or local variables)
    /// into MRTK data binding. Specifically for use with the DataConsumerLocalizedText.
    ///
    /// This allows us to add dynamic variables at runtime to use with localized texts.
    /// </summary>
    public class DataSourceLocalizationVariables : DataSourceGOBase
    {
        public const string DATA_SOURCE_TYPE = "locVariables";
        private Dictionary<string, IVariable> _localVariablesByID = new Dictionary<string, IVariable>();
        public Dictionary<string, IVariable> LocalVariablesByID => _localVariablesByID;

        protected override void InitializeDataSource()
        {
            dataSourceType = DATA_SOURCE_TYPE;
            Initialize(_localVariablesByID);
        }

        /// <summary>
        /// Initialize the data source with the local variables and notify all changed.
        /// </summary>
        /// <param name="localVariables"></param>
        public void Initialize(Dictionary<string, IVariable> localVariables)
        {
            if (localVariables != null)
            {
                _localVariablesByID = localVariables;
                DataSourceReflection dataSource = DataSource as DataSourceReflection;

                dataSource.SetDataSourceObject(localVariables);
                DataSource.NotifyAllChanged();
            }
        }
        
        /// </inheritdoc/>
        public override IDataSource AllocateDataSource()
        {
            return new DataSourceReflection(LocalVariablesByID);
        }
    }
}
