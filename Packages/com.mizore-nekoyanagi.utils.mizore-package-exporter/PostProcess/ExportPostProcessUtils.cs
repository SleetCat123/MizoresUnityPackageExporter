using UnityEngine;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
    public static class ExportPostProcessUtils {
        public static System.Type GetType( MizoresPackageExporter p ) {
            if ( !ExporterEditorPrefs.UsePostProcessScript ) {
                return null;
            }
            if ( string.IsNullOrEmpty( p.postProcessScriptTypeName ) ) {
                return null;
            }
            var type = System.Type.GetType( p.postProcessScriptTypeName );
            if ( type == null ) {
                Debug.LogError( ExporterTexts.PostProcessScriptNotFound( p.postProcessScriptTypeName ) );
                return null;
            }
            if ( !type.IsSubclassOf( typeof( ExportPostProcess ) ) ) {
                Debug.LogError( ExporterTexts.PostProcessScriptNotImplement );
                return null;
            }
            return type;
        }
        public class InstanceData {
            public System.Type type;
            public ExportPostProcess instance;
            public System.Reflection.FieldInfo[] fields;
        }
        public static InstanceData CreateInstance( MizoresPackageExporter p ) {
            var type = GetType( p );
            var fields = type.GetFields( System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance );
            // インスタンス化
            var instance = System.Activator.CreateInstance( type ) as ExportPostProcess;
            // フィールドに値を設定
            foreach ( var field in fields ) {
                string valueStr;
                if ( p.postProcessScriptFieldValues.TryGetValue( field.Name, out valueStr ) ) {
                    Debug.Log( $"Set field value: {field.Name} = {valueStr} ({field.FieldType})" );
                    object value;
                    if ( ExporterUtils.FromJson( valueStr, field.FieldType, out value ) ) {
                        field.SetValue( instance, value );
                    }
                }
            }
            return new InstanceData {
                type = type,
                instance = instance,
                fields = fields
            };
        }
    }
}
