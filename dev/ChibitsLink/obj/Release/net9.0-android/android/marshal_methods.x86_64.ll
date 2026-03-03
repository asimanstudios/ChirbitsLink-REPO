; ModuleID = 'marshal_methods.x86_64.ll'
source_filename = "marshal_methods.x86_64.ll"
target datalayout = "e-m:e-p270:32:32-p271:32:32-p272:64:64-i64:64-f80:128-n8:16:32:64-S128"
target triple = "x86_64-unknown-linux-android21"

%struct.MarshalMethodName = type {
	i64, ; uint64_t id
	ptr ; char* name
}

%struct.MarshalMethodsManagedClass = type {
	i32, ; uint32_t token
	ptr ; MonoClass klass
}

@assembly_image_cache = dso_local local_unnamed_addr global [179 x ptr] zeroinitializer, align 16

; Each entry maps hash of an assembly name to an index into the `assembly_image_cache` array
@assembly_image_cache_hashes = dso_local local_unnamed_addr constant [537 x i64] [
	i64 u0x0071cf2d27b7d61e, ; 0: lib_Xamarin.AndroidX.SwipeRefreshLayout.dll.so => 81
	i64 u0x02123411c4e01926, ; 1: lib_Xamarin.AndroidX.Navigation.Runtime.dll.so => 76
	i64 u0x02abedc11addc1ed, ; 2: lib_Mono.Android.Runtime.dll.so => 177
	i64 u0x032267b2a94db371, ; 3: lib_Xamarin.AndroidX.AppCompat.dll.so => 57
	i64 u0x043032f1d071fae0, ; 4: ru/Microsoft.Maui.Controls.resources => 24
	i64 u0x044440a55165631e, ; 5: lib-cs-Microsoft.Maui.Controls.resources.dll.so => 2
	i64 u0x046eb1581a80c6b0, ; 6: vi/Microsoft.Maui.Controls.resources => 30
	i64 u0x04acae429ea0efac, ; 7: Xamarin.Grpc.Context => 101
	i64 u0x0517ef04e06e9f76, ; 8: System.Net.Primitives => 139
	i64 u0x051a3be159e4ef99, ; 9: Xamarin.GooglePlayServices.Tasks => 98
	i64 u0x0565d18c6da3de38, ; 10: Xamarin.AndroidX.RecyclerView => 78
	i64 u0x0581db89237110e9, ; 11: lib_System.Collections.dll.so => 117
	i64 u0x05989cb940b225a9, ; 12: Microsoft.Maui.dll => 45
	i64 u0x06076b5d2b581f08, ; 13: zh-HK/Microsoft.Maui.Controls.resources => 31
	i64 u0x06388ffe9f6c161a, ; 14: System.Xml.Linq.dll => 170
	i64 u0x0680a433c781bb3d, ; 15: Xamarin.AndroidX.Collection.Jvm => 61
	i64 u0x07469f2eecce9e85, ; 16: mscorlib.dll => 174
	i64 u0x07c57877c7ba78ad, ; 17: ru/Microsoft.Maui.Controls.resources.dll => 24
	i64 u0x07dcdc7460a0c5e4, ; 18: System.Collections.NonGeneric => 115
	i64 u0x08a7c865576bbde7, ; 19: System.Reflection.Primitives => 153
	i64 u0x08f3c9788ee2153c, ; 20: Xamarin.AndroidX.DrawerLayout => 66
	i64 u0x0919c28b89381a0b, ; 21: lib_Microsoft.Extensions.Options.dll.so => 41
	i64 u0x092266563089ae3e, ; 22: lib_System.Collections.NonGeneric.dll.so => 115
	i64 u0x098b50f911ccea8d, ; 23: lib_Xamarin.GooglePlayServices.Basement.dll.so => 97
	i64 u0x09d144a7e214d457, ; 24: System.Security.Cryptography => 160
	i64 u0x09da6dfc3439e851, ; 25: lib_Xamarin.Firebase.Components.dll.so => 87
	i64 u0x0abb3e2b271edc45, ; 26: System.Threading.Channels.dll => 165
	i64 u0x0b3b632c3bbee20c, ; 27: sk/Microsoft.Maui.Controls.resources => 25
	i64 u0x0b6aff547b84fbe9, ; 28: Xamarin.KotlinX.Serialization.Core.Jvm => 110
	i64 u0x0be2e1f8ce4064ed, ; 29: Xamarin.AndroidX.ViewPager => 82
	i64 u0x0c3ca6cc978e2aae, ; 30: pt-BR/Microsoft.Maui.Controls.resources => 21
	i64 u0x0c59ad9fbbd43abe, ; 31: Mono.Android => 178
	i64 u0x0c7790f60165fc06, ; 32: lib_Microsoft.Maui.Essentials.dll.so => 46
	i64 u0x0cce4bce83380b7f, ; 33: Xamarin.AndroidX.Security.SecurityCrypto => 80
	i64 u0x0e14e73a54dda68e, ; 34: lib_System.Net.NameResolution.dll.so => 137
	i64 u0x102a31b45304b1da, ; 35: Xamarin.AndroidX.CustomView => 65
	i64 u0x1043d2dcf9fbfd22, ; 36: lib_Plugin.FirebaseAuth.dll.so => 52
	i64 u0x10f6cfcbcf801616, ; 37: System.IO.Compression.Brotli => 129
	i64 u0x11a70d0e1009fb11, ; 38: System.Net.WebSockets.dll => 145
	i64 u0x1225e20f72e93ce0, ; 39: ChibitsLink => 112
	i64 u0x123639456fb056da, ; 40: System.Reflection.Emit.Lightweight.dll => 152
	i64 u0x125b7f94acb989db, ; 41: Xamarin.AndroidX.RecyclerView.dll => 78
	i64 u0x13a01de0cbc3f06c, ; 42: lib-fr-Microsoft.Maui.Controls.resources.dll.so => 8
	i64 u0x13f1e5e209e91af4, ; 43: lib_Java.Interop.dll.so => 176
	i64 u0x13f1e880c25d96d1, ; 44: he/Microsoft.Maui.Controls.resources => 9
	i64 u0x143d8ea60a6a4011, ; 45: Microsoft.Extensions.DependencyInjection.Abstractions => 38
	i64 u0x1497051b917530bd, ; 46: lib_System.Net.WebSockets.dll.so => 145
	i64 u0x16726eac78495e6d, ; 47: Xamarin.Grpc.Stub => 105
	i64 u0x16c9d17b90a80fc1, ; 48: lib_Xamarin.Io.OpenCensus.OpenCensusApi.dll.so => 106
	i64 u0x1752c12f1e1fc00c, ; 49: System.Core => 122
	i64 u0x17b56e25558a5d36, ; 50: lib-hu-Microsoft.Maui.Controls.resources.dll.so => 12
	i64 u0x17f9358913beb16a, ; 51: System.Text.Encodings.Web => 162
	i64 u0x18402a709e357f3b, ; 52: lib_Xamarin.KotlinX.Serialization.Core.Jvm.dll.so => 110
	i64 u0x18f0ce884e87d89a, ; 53: nb/Microsoft.Maui.Controls.resources.dll => 18
	i64 u0x1a91866a319e9259, ; 54: lib_System.Collections.Concurrent.dll.so => 114
	i64 u0x1aac34d1917ba5d3, ; 55: lib_System.dll.so => 173
	i64 u0x1aad60783ffa3e5b, ; 56: lib-th-Microsoft.Maui.Controls.resources.dll.so => 27
	i64 u0x1c753b5ff15bce1b, ; 57: Mono.Android.Runtime.dll => 177
	i64 u0x1da4110562816681, ; 58: Xamarin.AndroidX.Security.SecurityCrypto.dll => 80
	i64 u0x1dcda680b17dc5bb, ; 59: lib_Xamarin.Google.Guava.FailureAccess.dll.so => 95
	i64 u0x1e3d87657e9659bc, ; 60: Xamarin.AndroidX.Navigation.UI => 77
	i64 u0x1e71143913d56c10, ; 61: lib-ko-Microsoft.Maui.Controls.resources.dll.so => 16
	i64 u0x1ed8fcce5e9b50a0, ; 62: Microsoft.Extensions.Options.dll => 41
	i64 u0x209375905fcc1bad, ; 63: lib_System.IO.Compression.Brotli.dll.so => 129
	i64 u0x2174319c0d835bc9, ; 64: System.Runtime => 159
	i64 u0x218ae22aa3ec33e7, ; 65: Xamarin.Grpc.Protobuf.Lite.dll => 104
	i64 u0x21cc7e445dcd5469, ; 66: System.Reflection.Emit.ILGeneration => 151
	i64 u0x220fd4f2e7c48170, ; 67: th/Microsoft.Maui.Controls.resources => 27
	i64 u0x2347c268e3e4e536, ; 68: Xamarin.GooglePlayServices.Basement.dll => 97
	i64 u0x237be844f1f812c7, ; 69: System.Threading.Thread.dll => 166
	i64 u0x2407aef2bbe8fadf, ; 70: System.Console => 121
	i64 u0x240abe014b27e7d3, ; 71: Xamarin.AndroidX.Core.dll => 63
	i64 u0x247619fe4413f8bf, ; 72: System.Runtime.Serialization.Primitives.dll => 158
	i64 u0x24b87318591adabe, ; 73: lib_Xamarin.Firebase.Database.Collection.dll.so => 88
	i64 u0x252073cc3caa62c2, ; 74: fr/Microsoft.Maui.Controls.resources.dll => 8
	i64 u0x256b8d41255f01b1, ; 75: Xamarin.Google.Crypto.Tink.Android => 93
	i64 u0x2662c629b96b0b30, ; 76: lib_Xamarin.Kotlin.StdLib.dll.so => 108
	i64 u0x268c1439f13bcc29, ; 77: lib_Microsoft.Extensions.Primitives.dll.so => 42
	i64 u0x26918e5f13c8fc7c, ; 78: Xamarin.Firebase.Firestore => 89
	i64 u0x273f3515de5faf0d, ; 79: id/Microsoft.Maui.Controls.resources.dll => 13
	i64 u0x2742545f9094896d, ; 80: hr/Microsoft.Maui.Controls.resources => 11
	i64 u0x2759af78ab94d39b, ; 81: System.Net.WebSockets => 145
	i64 u0x27b2b16f3e9de038, ; 82: Xamarin.Google.Crypto.Tink.Android.dll => 93
	i64 u0x27b410442fad6cf1, ; 83: Java.Interop.dll => 176
	i64 u0x27d88445c936a1af, ; 84: lib_Xamarin.Grpc.Android.dll.so => 99
	i64 u0x2801845a2c71fbfb, ; 85: System.Net.Primitives.dll => 139
	i64 u0x2a128783efe70ba0, ; 86: uk/Microsoft.Maui.Controls.resources.dll => 29
	i64 u0x2a3b095612184159, ; 87: lib_System.Net.NetworkInformation.dll.so => 138
	i64 u0x2a6507a5ffabdf28, ; 88: System.Diagnostics.TraceSource.dll => 125
	i64 u0x2ad156c8e1354139, ; 89: fi/Microsoft.Maui.Controls.resources => 7
	i64 u0x2af298f63581d886, ; 90: System.Text.RegularExpressions.dll => 164
	i64 u0x2afc1c4f898552ee, ; 91: lib_System.Formats.Asn1.dll.so => 128
	i64 u0x2b0f316f4c87d83a, ; 92: Xamarin.Io.OpenCensus.OpenCensusApi => 106
	i64 u0x2b148910ed40fbf9, ; 93: zh-Hant/Microsoft.Maui.Controls.resources.dll => 33
	i64 u0x2c8bd14bb93a7d82, ; 94: lib-pl-Microsoft.Maui.Controls.resources.dll.so => 20
	i64 u0x2cc9e1fed6257257, ; 95: lib_System.Reflection.Emit.Lightweight.dll.so => 152
	i64 u0x2cd723e9fe623c7c, ; 96: lib_System.Private.Xml.Linq.dll.so => 149
	i64 u0x2d169d318a968379, ; 97: System.Threading.dll => 168
	i64 u0x2d1d1413dd10c597, ; 98: Xamarin.Google.Guava.FailureAccess => 95
	i64 u0x2d47774b7d993f59, ; 99: sv/Microsoft.Maui.Controls.resources.dll => 26
	i64 u0x2d6267ac7de1d619, ; 100: Xamarin.Firebase.Database.Collection.dll => 88
	i64 u0x2db915caf23548d2, ; 101: System.Text.Json.dll => 163
	i64 u0x2e6f1f226821322a, ; 102: el/Microsoft.Maui.Controls.resources.dll => 5
	i64 u0x2e958faf35bfcf82, ; 103: Plugin.BLE.dll => 49
	i64 u0x2f2e98e1c89b1aff, ; 104: System.Xml.ReaderWriter => 171
	i64 u0x2fd92a71c7638cfd, ; 105: Xamarin.Firebase.Database.Collection => 88
	i64 u0x309ee9eeec09a71e, ; 106: lib_Xamarin.AndroidX.Fragment.dll.so => 67
	i64 u0x31195fef5d8fb552, ; 107: _Microsoft.Android.Resource.Designer.dll => 34
	i64 u0x32243413e774362a, ; 108: Xamarin.AndroidX.CardView.dll => 59
	i64 u0x3235427f8d12dae1, ; 109: lib_System.Drawing.Primitives.dll.so => 126
	i64 u0x329753a17a517811, ; 110: fr/Microsoft.Maui.Controls.resources => 8
	i64 u0x32aa989ff07a84ff, ; 111: lib_System.Xml.ReaderWriter.dll.so => 171
	i64 u0x33a31443733849fe, ; 112: lib-es-Microsoft.Maui.Controls.resources.dll.so => 6
	i64 u0x341abc357fbb4ebf, ; 113: lib_System.Net.Sockets.dll.so => 142
	i64 u0x342397b849d48e49, ; 114: Xamarin.Grpc.Core => 102
	i64 u0x34dfd74fe2afcf37, ; 115: Microsoft.Maui => 45
	i64 u0x34e292762d9615df, ; 116: cs/Microsoft.Maui.Controls.resources.dll => 2
	i64 u0x35080e71f38b333d, ; 117: Xamarin.Protobuf.Lite => 111
	i64 u0x3508234247f48404, ; 118: Microsoft.Maui.Controls => 43
	i64 u0x353c74869339570c, ; 119: lib_Xamarin.Firebase.Auth.dll.so => 84
	i64 u0x3549870798b4cd30, ; 120: lib_Xamarin.AndroidX.ViewPager2.dll.so => 83
	i64 u0x355282fc1c909694, ; 121: Microsoft.Extensions.Configuration => 35
	i64 u0x356fd122ba041cb4, ; 122: lib_Xamarin.Grpc.Protobuf.Lite.dll.so => 104
	i64 u0x364703ab05867b92, ; 123: Xamarin.Firebase.Components => 87
	i64 u0x36b2b50fdf589ae2, ; 124: System.Reflection.Emit.Lightweight => 152
	i64 u0x374454d3b00bcc2f, ; 125: Plugin.CurrentActivity.dll => 51
	i64 u0x374ef46b06791af6, ; 126: System.Reflection.Primitives.dll => 153
	i64 u0x385c17636bb6fe6e, ; 127: Xamarin.AndroidX.CustomView.dll => 65
	i64 u0x38869c811d74050e, ; 128: System.Net.NameResolution.dll => 137
	i64 u0x393c226616977fdb, ; 129: lib_Xamarin.AndroidX.ViewPager.dll.so => 82
	i64 u0x395b3053dde89e41, ; 130: lib_System.Reactive.dll.so => 55
	i64 u0x395e37c3334cf82a, ; 131: lib-ca-Microsoft.Maui.Controls.resources.dll.so => 1
	i64 u0x39a87563fdb248a0, ; 132: System.Reactive.dll => 55
	i64 u0x39aa39fda111d9d3, ; 133: Newtonsoft.Json => 48
	i64 u0x3a6d66cb8ab50418, ; 134: Plugin.FirebaseAuth => 52
	i64 u0x3b860f9932505633, ; 135: lib_System.Text.Encoding.Extensions.dll.so => 161
	i64 u0x3c7c495f58ac5ee9, ; 136: Xamarin.Kotlin.StdLib => 108
	i64 u0x3c90a7b70f45292a, ; 137: Xamarin.Grpc.OkHttp.dll => 103
	i64 u0x3cc1676a8781bdbc, ; 138: Xamarin.Firebase.Auth.Interop.dll => 85
	i64 u0x3d2b1913edfc08d7, ; 139: lib_System.Threading.ThreadPool.dll.so => 167
	i64 u0x3d46f0b995082740, ; 140: System.Xml.Linq => 170
	i64 u0x3d9c2a242b040a50, ; 141: lib_Xamarin.AndroidX.Core.dll.so => 63
	i64 u0x407a10bb4bf95829, ; 142: lib_Xamarin.AndroidX.Navigation.Common.dll.so => 74
	i64 u0x41833cf766d27d96, ; 143: mscorlib => 174
	i64 u0x41cab042be111c34, ; 144: lib_Xamarin.AndroidX.AppCompat.AppCompatResources.dll.so => 58
	i64 u0x434c4e1d9284cdae, ; 145: Mono.Android.dll => 178
	i64 u0x43950f84de7cc79a, ; 146: pl/Microsoft.Maui.Controls.resources.dll => 20
	i64 u0x448bd33429269b19, ; 147: Microsoft.CSharp => 113
	i64 u0x4499fa3c8e494654, ; 148: lib_System.Runtime.Serialization.Primitives.dll.so => 158
	i64 u0x4515080865a951a5, ; 149: Xamarin.Kotlin.StdLib.dll => 108
	i64 u0x45c40276a42e283e, ; 150: System.Diagnostics.TraceSource => 125
	i64 u0x46a4213bc97fe5ae, ; 151: lib-ru-Microsoft.Maui.Controls.resources.dll.so => 24
	i64 u0x46f84b5cc9b7d78b, ; 152: Xamarin.Io.OpenCensus.OpenCensusContribGrpcMetrics.dll => 107
	i64 u0x47358bd471172e1d, ; 153: lib_System.Xml.Linq.dll.so => 170
	i64 u0x47c5d5a96aac3cd1, ; 154: Plugin.CurrentActivity => 51
	i64 u0x47daf4e1afbada10, ; 155: pt/Microsoft.Maui.Controls.resources => 22
	i64 u0x49e952f19a4e2022, ; 156: System.ObjectModel => 147
	i64 u0x49f6ab815e178ca9, ; 157: lib_Xamarin.Firebase.Common.dll.so => 86
	i64 u0x4a0fc182a3c7fc42, ; 158: Xamarin.Protobuf.Lite.dll => 111
	i64 u0x4a5667b2462a664b, ; 159: lib_Xamarin.AndroidX.Navigation.UI.dll.so => 77
	i64 u0x4b7b6532ded934b7, ; 160: System.Text.Json => 163
	i64 u0x4cc5f15266470798, ; 161: lib_Xamarin.AndroidX.Loader.dll.so => 72
	i64 u0x4cf6f67dc77aacd2, ; 162: System.Net.NetworkInformation.dll => 138
	i64 u0x4d3183dd245425d4, ; 163: System.Net.WebSockets.Client.dll => 144
	i64 u0x4d479f968a05e504, ; 164: System.Linq.Expressions.dll => 132
	i64 u0x4d55a010ffc4faff, ; 165: System.Private.Xml => 150
	i64 u0x4d95fccc1f67c7ca, ; 166: System.Runtime.Loader.dll => 155
	i64 u0x4dcf44c3c9b076a2, ; 167: it/Microsoft.Maui.Controls.resources.dll => 14
	i64 u0x4dd9247f1d2c3235, ; 168: Xamarin.AndroidX.Loader.dll => 72
	i64 u0x4e32f00cb0937401, ; 169: Mono.Android.Runtime => 177
	i64 u0x4ebd0c4b82c5eefc, ; 170: lib_System.Threading.Channels.dll.so => 165
	i64 u0x4f21ee6ef9eb527e, ; 171: ca/Microsoft.Maui.Controls.resources => 1
	i64 u0x4ff8ea8951a69b9f, ; 172: Xamarin.Grpc.Android.dll => 99
	i64 u0x5037f0be3c28c7a3, ; 173: lib_Microsoft.Maui.Controls.dll.so => 43
	i64 u0x506203448c473a65, ; 174: Xamarin.Google.AutoValue.Annotations => 92
	i64 u0x5131bbe80989093f, ; 175: Xamarin.AndroidX.Lifecycle.ViewModel.Android.dll => 70
	i64 u0x515d61d6527dac70, ; 176: lib_Xamarin.Firebase.Auth.Interop.dll.so => 85
	i64 u0x51bb8a2afe774e32, ; 177: System.Drawing => 127
	i64 u0x526ce79eb8e90527, ; 178: lib_System.Net.Primitives.dll.so => 139
	i64 u0x52829f00b4467c38, ; 179: lib_System.Data.Common.dll.so => 123
	i64 u0x529ffe06f39ab8db, ; 180: Xamarin.AndroidX.Core => 63
	i64 u0x52ff996554dbf352, ; 181: Microsoft.Maui.Graphics => 47
	i64 u0x535f7e40e8fef8af, ; 182: lib-sk-Microsoft.Maui.Controls.resources.dll.so => 25
	i64 u0x53a96d5c86c9e194, ; 183: System.Net.NetworkInformation => 138
	i64 u0x53c3014b9437e684, ; 184: lib-zh-HK-Microsoft.Maui.Controls.resources.dll.so => 31
	i64 u0x53e450ebd586f842, ; 185: lib_Xamarin.AndroidX.LocalBroadcastManager.dll.so => 73
	i64 u0x54795225dd1587af, ; 186: lib_System.Runtime.dll.so => 159
	i64 u0x556e8b63b660ab8b, ; 187: Xamarin.AndroidX.Lifecycle.Common.Jvm.dll => 68
	i64 u0x5588627c9a108ec9, ; 188: System.Collections.Specialized => 116
	i64 u0x571c5cfbec5ae8e2, ; 189: System.Private.Uri => 148
	i64 u0x57201164aeb974e3, ; 190: Xamarin.Google.Guava.FailureAccess.dll => 95
	i64 u0x579a06fed6eec900, ; 191: System.Private.CoreLib.dll => 175
	i64 u0x57c542c14049b66d, ; 192: System.Diagnostics.DiagnosticSource => 124
	i64 u0x58601b2dda4a27b9, ; 193: lib-ja-Microsoft.Maui.Controls.resources.dll.so => 15
	i64 u0x58688d9af496b168, ; 194: Microsoft.Extensions.DependencyInjection.dll => 37
	i64 u0x595a356d23e8da9a, ; 195: lib_Microsoft.CSharp.dll.so => 113
	i64 u0x5a6dc081b000c5d7, ; 196: lib_Xamarin.Grpc.OkHttp.dll.so => 103
	i64 u0x5a89a886ae30258d, ; 197: lib_Xamarin.AndroidX.CoordinatorLayout.dll.so => 62
	i64 u0x5a8f6699f4a1caa9, ; 198: lib_System.Threading.dll.so => 168
	i64 u0x5ae9cd33b15841bf, ; 199: System.ComponentModel => 120
	i64 u0x5aeb8cd498d4823e, ; 200: lib_Xamarin.Google.Guava.dll.so => 94
	i64 u0x5b5f0e240a06a2a2, ; 201: da/Microsoft.Maui.Controls.resources.dll => 3
	i64 u0x5b755276902c8414, ; 202: Xamarin.GooglePlayServices.Base => 96
	i64 u0x5bdf16b09da116ab, ; 203: Xamarin.AndroidX.Collection => 60
	i64 u0x5c393624b8176517, ; 204: lib_Microsoft.Extensions.Logging.dll.so => 39
	i64 u0x5d0a4a29b02d9d3c, ; 205: System.Net.WebHeaderCollection.dll => 143
	i64 u0x5db0cbbd1028510e, ; 206: lib_System.Runtime.InteropServices.dll.so => 154
	i64 u0x5db30905d3e5013b, ; 207: Xamarin.AndroidX.Collection.Jvm.dll => 61
	i64 u0x5e467bc8f09ad026, ; 208: System.Collections.Specialized.dll => 116
	i64 u0x5ea92fdb19ec8c4c, ; 209: System.Text.Encodings.Web.dll => 162
	i64 u0x5eb8046dd40e9ac3, ; 210: System.ComponentModel.Primitives => 118
	i64 u0x5f36ccf5c6a57e24, ; 211: System.Xml.ReaderWriter.dll => 171
	i64 u0x5f4294b9b63cb842, ; 212: System.Data.Common => 123
	i64 u0x5f9a2d823f664957, ; 213: lib-el-Microsoft.Maui.Controls.resources.dll.so => 5
	i64 u0x5fcdf154394efd90, ; 214: lib_Plugin.BLE.dll.so => 49
	i64 u0x609f4b7b63d802d4, ; 215: lib_Microsoft.Extensions.DependencyInjection.dll.so => 37
	i64 u0x60cd4e33d7e60134, ; 216: Xamarin.KotlinX.Coroutines.Core.Jvm => 109
	i64 u0x60f62d786afcf130, ; 217: System.Memory => 135
	i64 u0x61be8d1299194243, ; 218: Microsoft.Maui.Controls.Xaml => 44
	i64 u0x61d2cba29557038f, ; 219: de/Microsoft.Maui.Controls.resources => 4
	i64 u0x61d88f399afb2f45, ; 220: lib_System.Runtime.Loader.dll.so => 155
	i64 u0x622eef6f9e59068d, ; 221: System.Private.CoreLib => 175
	i64 u0x62e976fd765a2339, ; 222: Xamarin.Firebase.Auth.Interop => 85
	i64 u0x63982c87366f9be8, ; 223: Xamarin.Google.Guava => 94
	i64 u0x6400f68068c1e9f1, ; 224: Xamarin.Google.Android.Material.dll => 91
	i64 u0x65ecac39144dd3cc, ; 225: Microsoft.Maui.Controls.dll => 43
	i64 u0x65ece51227bfa724, ; 226: lib_System.Runtime.Numerics.dll.so => 156
	i64 u0x6692e924eade1b29, ; 227: lib_System.Console.dll.so => 121
	i64 u0x66a4e5c6a3fb0bae, ; 228: lib_Xamarin.AndroidX.Lifecycle.ViewModel.Android.dll.so => 70
	i64 u0x66d13304ce1a3efa, ; 229: Xamarin.AndroidX.CursorAdapter => 64
	i64 u0x68558ec653afa616, ; 230: lib-da-Microsoft.Maui.Controls.resources.dll.so => 3
	i64 u0x6872ec7a2e36b1ac, ; 231: System.Drawing.Primitives.dll => 126
	i64 u0x68fbbbe2eb455198, ; 232: System.Formats.Asn1 => 128
	i64 u0x69063fc0ba8e6bdd, ; 233: he/Microsoft.Maui.Controls.resources.dll => 9
	i64 u0x6a4d7577b2317255, ; 234: System.Runtime.InteropServices.dll => 154
	i64 u0x6ace3b74b15ee4a4, ; 235: nb/Microsoft.Maui.Controls.resources => 18
	i64 u0x6afcedb171067e2b, ; 236: System.Core.dll => 122
	i64 u0x6b2b13561049ea2c, ; 237: lib_Xamarin.Protobuf.Lite.dll.so => 111
	i64 u0x6d12bfaa99c72b1f, ; 238: lib_Microsoft.Maui.Graphics.dll.so => 47
	i64 u0x6d79993361e10ef2, ; 239: Microsoft.Extensions.Primitives => 42
	i64 u0x6d86d56b84c8eb71, ; 240: lib_Xamarin.AndroidX.CursorAdapter.dll.so => 64
	i64 u0x6d9bea6b3e895cf7, ; 241: Microsoft.Extensions.Primitives.dll => 42
	i64 u0x6e25a02c3833319a, ; 242: lib_Xamarin.AndroidX.Navigation.Fragment.dll.so => 75
	i64 u0x6e9965ce1095e60a, ; 243: lib_System.Core.dll.so => 122
	i64 u0x6fd2265da78b93a4, ; 244: lib_Microsoft.Maui.dll.so => 45
	i64 u0x6fdfc7de82c33008, ; 245: cs/Microsoft.Maui.Controls.resources => 2
	i64 u0x70664ad3307f4fbf, ; 246: Xamarin.Grpc.Core.dll => 102
	i64 u0x70e99f48c05cb921, ; 247: tr/Microsoft.Maui.Controls.resources.dll => 28
	i64 u0x70fb7a3521043a40, ; 248: Plugin.CloudFirestore => 50
	i64 u0x70fd3deda22442d2, ; 249: lib-nb-Microsoft.Maui.Controls.resources.dll.so => 18
	i64 u0x71a495ea3761dde8, ; 250: lib-it-Microsoft.Maui.Controls.resources.dll.so => 14
	i64 u0x71ad672adbe48f35, ; 251: System.ComponentModel.Primitives.dll => 118
	i64 u0x72b1fb4109e08d7b, ; 252: lib-hr-Microsoft.Maui.Controls.resources.dll.so => 11
	i64 u0x73e4ce94e2eb6ffc, ; 253: lib_System.Memory.dll.so => 135
	i64 u0x755a91767330b3d4, ; 254: lib_Microsoft.Extensions.Configuration.dll.so => 35
	i64 u0x76012e7334db86e5, ; 255: lib_Xamarin.AndroidX.SavedState.dll.so => 79
	i64 u0x76ca07b878f44da0, ; 256: System.Runtime.Numerics.dll => 156
	i64 u0x77bf40592cd67602, ; 257: Xamarin.Google.AutoValue.Annotations.dll => 92
	i64 u0x77d48bf846bc0f10, ; 258: Xamarin.Io.OpenCensus.OpenCensusContribGrpcMetrics => 107
	i64 u0x780bc73597a503a9, ; 259: lib-ms-Microsoft.Maui.Controls.resources.dll.so => 17
	i64 u0x783606d1e53e7a1a, ; 260: th/Microsoft.Maui.Controls.resources.dll => 27
	i64 u0x784b4ff3eed363ff, ; 261: Xamarin.Firebase.Common => 86
	i64 u0x78a45e51311409b6, ; 262: Xamarin.AndroidX.Fragment.dll => 67
	i64 u0x7939c1796e1e9b03, ; 263: Square.OkHttp.dll => 53
	i64 u0x7ad0f4f1e5d08183, ; 264: Xamarin.AndroidX.Collection.dll => 60
	i64 u0x7adb8da2ac89b647, ; 265: fi/Microsoft.Maui.Controls.resources.dll => 7
	i64 u0x7bef86a4335c4870, ; 266: System.ComponentModel.TypeConverter => 119
	i64 u0x7c0820144cd34d6a, ; 267: sk/Microsoft.Maui.Controls.resources.dll => 25
	i64 u0x7c2a0bd1e0f988fc, ; 268: lib-de-Microsoft.Maui.Controls.resources.dll.so => 4
	i64 u0x7c8cb8cf04bee12b, ; 269: lib_Xamarin.Google.AutoValue.Annotations.dll.so => 92
	i64 u0x7cb95ad2a929d044, ; 270: Xamarin.GooglePlayServices.Basement => 97
	i64 u0x7d649b75d580bb42, ; 271: ms/Microsoft.Maui.Controls.resources.dll => 17
	i64 u0x7d8ee2bdc8e3aad1, ; 272: System.Numerics.Vectors => 146
	i64 u0x7dfc3d6d9d8d7b70, ; 273: System.Collections => 117
	i64 u0x7e65340ed0da76d2, ; 274: Xamarin.Grpc.OkHttp => 103
	i64 u0x7e946809d6008ef2, ; 275: lib_System.ObjectModel.dll.so => 147
	i64 u0x7eb4f0dc47488736, ; 276: lib_Xamarin.GooglePlayServices.Tasks.dll.so => 98
	i64 u0x7ecc13347c8fd849, ; 277: lib_System.ComponentModel.dll.so => 120
	i64 u0x7f00ddd9b9ca5a13, ; 278: Xamarin.AndroidX.ViewPager.dll => 82
	i64 u0x7f9351cd44b1273f, ; 279: Microsoft.Extensions.Configuration.Abstractions => 36
	i64 u0x7fbd557c99b3ce6f, ; 280: lib_Xamarin.AndroidX.Lifecycle.LiveData.Core.dll.so => 69
	i64 u0x812c069d5cdecc17, ; 281: System.dll => 173
	i64 u0x81ab745f6c0f5ce6, ; 282: zh-Hant/Microsoft.Maui.Controls.resources => 33
	i64 u0x8277f2be6b5ce05f, ; 283: Xamarin.AndroidX.AppCompat => 57
	i64 u0x828f06563b30bc50, ; 284: lib_Xamarin.AndroidX.CardView.dll.so => 59
	i64 u0x82df8f5532a10c59, ; 285: lib_System.Drawing.dll.so => 127
	i64 u0x82f6403342e12049, ; 286: uk/Microsoft.Maui.Controls.resources => 29
	i64 u0x83c14ba66c8e2b8c, ; 287: zh-Hans/Microsoft.Maui.Controls.resources => 32
	i64 u0x850c5ba0b57ce8e7, ; 288: lib_Xamarin.AndroidX.Collection.dll.so => 60
	i64 u0x85410a0ce2b82e74, ; 289: lib_Xamarin.Grpc.Context.dll.so => 101
	i64 u0x866c029b39d9cde6, ; 290: Plugin.CloudFirestore.dll => 50
	i64 u0x86a909228dc7657b, ; 291: lib-zh-Hant-Microsoft.Maui.Controls.resources.dll.so => 33
	i64 u0x86b3e00c36b84509, ; 292: Microsoft.Extensions.Configuration.dll => 35
	i64 u0x8794c7c19600413d, ; 293: Xamarin.Grpc.Protobuf.Lite => 104
	i64 u0x87b7bede2c8fef74, ; 294: Xamarin.Firebase.ProtoliteWellKnownTypes.dll => 90
	i64 u0x87c69b87d9283884, ; 295: lib_System.Threading.Thread.dll.so => 166
	i64 u0x87f6569b25707834, ; 296: System.IO.Compression.Brotli.dll => 129
	i64 u0x8842b3a5d2d3fb36, ; 297: Microsoft.Maui.Essentials => 46
	i64 u0x88bda98e0cffb7a9, ; 298: lib_Xamarin.KotlinX.Coroutines.Core.Jvm.dll.so => 109
	i64 u0x897a606c9e39c75f, ; 299: lib_System.ComponentModel.Primitives.dll.so => 118
	i64 u0x8ad229ea26432ee2, ; 300: Xamarin.AndroidX.Loader => 72
	i64 u0x8b4ff5d0fdd5faa1, ; 301: lib_System.Diagnostics.DiagnosticSource.dll.so => 124
	i64 u0x8b9ceca7acae3451, ; 302: lib-he-Microsoft.Maui.Controls.resources.dll.so => 9
	i64 u0x8c230514448fef34, ; 303: Xamarin.Firebase.ProtoliteWellKnownTypes => 90
	i64 u0x8d0f420977c2c1c7, ; 304: Xamarin.AndroidX.CursorAdapter.dll => 64
	i64 u0x8d7b8ab4b3310ead, ; 305: System.Threading => 168
	i64 u0x8da188285aadfe8e, ; 306: System.Collections.Concurrent => 114
	i64 u0x8e68459d22cb214f, ; 307: Square.OkHttp => 53
	i64 u0x8ec6e06a61c1baeb, ; 308: lib_Newtonsoft.Json.dll.so => 48
	i64 u0x8ed807bfe9858dfc, ; 309: Xamarin.AndroidX.Navigation.Common => 74
	i64 u0x8ee08b8194a30f48, ; 310: lib-hi-Microsoft.Maui.Controls.resources.dll.so => 10
	i64 u0x8ef7601039857a44, ; 311: lib-ro-Microsoft.Maui.Controls.resources.dll.so => 23
	i64 u0x8efbc0801a122264, ; 312: Xamarin.GooglePlayServices.Tasks.dll => 98
	i64 u0x8f32c6f611f6ffab, ; 313: pt/Microsoft.Maui.Controls.resources.dll => 22
	i64 u0x8f8829d21c8985a4, ; 314: lib-pt-BR-Microsoft.Maui.Controls.resources.dll.so => 21
	i64 u0x8fd42635e63de49e, ; 315: Xamarin.Grpc.Context.dll => 101
	i64 u0x90263f8448b8f572, ; 316: lib_System.Diagnostics.TraceSource.dll.so => 125
	i64 u0x903101b46fb73a04, ; 317: _Microsoft.Android.Resource.Designer => 34
	i64 u0x90393bd4865292f3, ; 318: lib_System.IO.Compression.dll.so => 130
	i64 u0x90634f86c5ebe2b5, ; 319: Xamarin.AndroidX.Lifecycle.ViewModel.Android => 70
	i64 u0x907b636704ad79ef, ; 320: lib_Microsoft.Maui.Controls.Xaml.dll.so => 44
	i64 u0x91413101e5d9f995, ; 321: Xamarin.Firebase.Auth => 84
	i64 u0x91418dc638b29e68, ; 322: lib_Xamarin.AndroidX.CustomView.dll.so => 65
	i64 u0x9157bd523cd7ed36, ; 323: lib_System.Text.Json.dll.so => 163
	i64 u0x91a74f07b30d37e2, ; 324: System.Linq.dll => 134
	i64 u0x91fa41a87223399f, ; 325: ca/Microsoft.Maui.Controls.resources.dll => 1
	i64 u0x92a698e6d582778f, ; 326: Xamarin.Firebase.Components.dll => 87
	i64 u0x93cfa73ab28d6e35, ; 327: ms/Microsoft.Maui.Controls.resources => 17
	i64 u0x944077d8ca3c6580, ; 328: System.IO.Compression.dll => 130
	i64 u0x967fc325e09bfa8c, ; 329: es/Microsoft.Maui.Controls.resources => 6
	i64 u0x9732d8dbddea3d9a, ; 330: id/Microsoft.Maui.Controls.resources => 13
	i64 u0x978be80e5210d31b, ; 331: Microsoft.Maui.Graphics.dll => 47
	i64 u0x979ab54025cc1c7f, ; 332: lib_Xamarin.GooglePlayServices.Base.dll.so => 96
	i64 u0x97b8c771ea3e4220, ; 333: System.ComponentModel.dll => 120
	i64 u0x97e144c9d3c6976e, ; 334: System.Collections.Concurrent.dll => 114
	i64 u0x991d510397f92d9d, ; 335: System.Linq.Expressions => 132
	i64 u0x99a00ca5270c6878, ; 336: Xamarin.AndroidX.Navigation.Runtime => 76
	i64 u0x99cdc6d1f2d3a72f, ; 337: ko/Microsoft.Maui.Controls.resources.dll => 16
	i64 u0x9a2d4c8408e9f4b6, ; 338: lib_Plugin.CloudFirestore.dll.so => 50
	i64 u0x9d5dbcf5a48583fe, ; 339: lib_Xamarin.AndroidX.Activity.dll.so => 56
	i64 u0x9d74dee1a7725f34, ; 340: Microsoft.Extensions.Configuration.Abstractions.dll => 36
	i64 u0x9e4534b6adaf6e84, ; 341: nl/Microsoft.Maui.Controls.resources => 19
	i64 u0x9eaf1efdf6f7267e, ; 342: Xamarin.AndroidX.Navigation.Common.dll => 74
	i64 u0x9ef542cf1f78c506, ; 343: Xamarin.AndroidX.Lifecycle.LiveData.Core => 69
	i64 u0x9f2c1126c41c6e52, ; 344: lib_Xamarin.Io.OpenCensus.OpenCensusContribGrpcMetrics.dll.so => 107
	i64 u0xa0d8259f4cc284ec, ; 345: lib_System.Security.Cryptography.dll.so => 160
	i64 u0xa0e17ca50c77a225, ; 346: lib_Xamarin.Google.Crypto.Tink.Android.dll.so => 93
	i64 u0xa1440773ee9d341e, ; 347: Xamarin.Google.Android.Material => 91
	i64 u0xa1b9d7c27f47219f, ; 348: Xamarin.AndroidX.Navigation.UI.dll => 77
	i64 u0xa2572680829d2c7c, ; 349: System.IO.Pipelines.dll => 131
	i64 u0xa308401900e5bed3, ; 350: lib_mscorlib.dll.so => 174
	i64 u0xa46aa1eaa214539b, ; 351: ko/Microsoft.Maui.Controls.resources => 16
	i64 u0xa4edc8f2ceae241a, ; 352: System.Data.Common.dll => 123
	i64 u0xa5494f40f128ce6a, ; 353: System.Runtime.Serialization.Formatters.dll => 157
	i64 u0xa5e599d1e0524750, ; 354: System.Numerics.Vectors.dll => 146
	i64 u0xa5f1ba49b85dd355, ; 355: System.Security.Cryptography.dll => 160
	i64 u0xa67dbee13e1df9ca, ; 356: Xamarin.AndroidX.SavedState.dll => 79
	i64 u0xa684b098dd27b296, ; 357: lib_Xamarin.AndroidX.Security.SecurityCrypto.dll.so => 80
	i64 u0xa68a420042bb9b1f, ; 358: Xamarin.AndroidX.DrawerLayout.dll => 66
	i64 u0xa78ce3745383236a, ; 359: Xamarin.AndroidX.Lifecycle.Common.Jvm => 68
	i64 u0xa7c31b56b4dc7b33, ; 360: hu/Microsoft.Maui.Controls.resources => 12
	i64 u0xa843f6095f0d247d, ; 361: Xamarin.GooglePlayServices.Base.dll => 96
	i64 u0xaa2219c8e3449ff5, ; 362: Microsoft.Extensions.Logging.Abstractions => 40
	i64 u0xaa443ac34067eeef, ; 363: System.Private.Xml.dll => 150
	i64 u0xaa52de307ef5d1dd, ; 364: System.Net.Http => 136
	i64 u0xaaaf86367285a918, ; 365: Microsoft.Extensions.DependencyInjection.Abstractions.dll => 38
	i64 u0xaaf84bb3f052a265, ; 366: el/Microsoft.Maui.Controls.resources => 5
	i64 u0xab9c1b2687d86b0b, ; 367: lib_System.Linq.Expressions.dll.so => 132
	i64 u0xac2af3fa195a15ce, ; 368: System.Runtime.Numerics => 156
	i64 u0xac5376a2a538dc10, ; 369: Xamarin.AndroidX.Lifecycle.LiveData.Core.dll => 69
	i64 u0xac98d31068e24591, ; 370: System.Xml.XDocument => 172
	i64 u0xacd46e002c3ccb97, ; 371: ro/Microsoft.Maui.Controls.resources => 23
	i64 u0xacf42eea7ef9cd12, ; 372: System.Threading.Channels => 165
	i64 u0xad89c07347f1bad6, ; 373: nl/Microsoft.Maui.Controls.resources.dll => 19
	i64 u0xad92c1702df5ca37, ; 374: Plugin.FirebaseAuth.dll => 52
	i64 u0xadbb53caf78a79d2, ; 375: System.Web.HttpUtility => 169
	i64 u0xadc90ab061a9e6e4, ; 376: System.ComponentModel.TypeConverter.dll => 119
	i64 u0xadf511667bef3595, ; 377: System.Net.Security => 141
	i64 u0xae282bcd03739de7, ; 378: Java.Interop => 176
	i64 u0xae53579c90db1107, ; 379: System.ObjectModel.dll => 147
	i64 u0xaec5f16f1e60a123, ; 380: lib_Plugin.CurrentActivity.dll.so => 51
	i64 u0xafe29f45095518e7, ; 381: lib_Xamarin.AndroidX.Lifecycle.ViewModelSavedState.dll.so => 71
	i64 u0xb05cc42cd94c6d9d, ; 382: lib-sv-Microsoft.Maui.Controls.resources.dll.so => 26
	i64 u0xb220631954820169, ; 383: System.Text.RegularExpressions => 164
	i64 u0xb2a3f67f3bf29fce, ; 384: da/Microsoft.Maui.Controls.resources => 3
	i64 u0xb3f0a0fcda8d3ebc, ; 385: Xamarin.AndroidX.CardView => 59
	i64 u0xb404462cdf3bffdb, ; 386: lib_Xamarin.Firebase.ProtoliteWellKnownTypes.dll.so => 90
	i64 u0xb46be1aa6d4fff93, ; 387: hi/Microsoft.Maui.Controls.resources => 10
	i64 u0xb477491be13109d8, ; 388: ar/Microsoft.Maui.Controls.resources => 0
	i64 u0xb4bd7015ecee9d86, ; 389: System.IO.Pipelines => 131
	i64 u0xb5c7fcdafbc67ee4, ; 390: Microsoft.Extensions.Logging.Abstractions.dll => 40
	i64 u0xb5ea31d5244c6626, ; 391: System.Threading.ThreadPool.dll => 167
	i64 u0xb7212c4683a94afe, ; 392: System.Drawing.Primitives => 126
	i64 u0xb7b7753d1f319409, ; 393: sv/Microsoft.Maui.Controls.resources => 26
	i64 u0xb81a2c6e0aee50fe, ; 394: lib_System.Private.CoreLib.dll.so => 175
	i64 u0xb9185c33a1643eed, ; 395: Microsoft.CSharp.dll => 113
	i64 u0xb9f64d3b230def68, ; 396: lib-pt-Microsoft.Maui.Controls.resources.dll.so => 22
	i64 u0xb9fc3c8a556e3691, ; 397: ja/Microsoft.Maui.Controls.resources => 15
	i64 u0xba4670aa94a2b3c6, ; 398: lib_System.Xml.XDocument.dll.so => 172
	i64 u0xba48785529705af9, ; 399: System.Collections.dll => 117
	i64 u0xbac8fedd220eef4e, ; 400: ChibitsLink.dll => 112
	i64 u0xbb65706fde942ce3, ; 401: System.Net.Sockets => 142
	i64 u0xbbd180354b67271a, ; 402: System.Runtime.Serialization.Formatters => 157
	i64 u0xbc0e640e7c6bcdf8, ; 403: Xamarin.Grpc.Api => 100
	i64 u0xbd06af7c5ae3690a, ; 404: lib_ChibitsLink.dll.so => 112
	i64 u0xbd0e2c0d55246576, ; 405: System.Net.Http.dll => 136
	i64 u0xbd437a2cdb333d0d, ; 406: Xamarin.AndroidX.ViewPager2 => 83
	i64 u0xbd854cde2dba71a3, ; 407: lib_Square.OkHttp.dll.so => 53
	i64 u0xbee38d4a88835966, ; 408: Xamarin.AndroidX.AppCompat.AppCompatResources => 58
	i64 u0xc040a4ab55817f58, ; 409: ar/Microsoft.Maui.Controls.resources.dll => 0
	i64 u0xc0c274b1d0a94d01, ; 410: Plugin.BLE => 49
	i64 u0xc0d928351ab5ca77, ; 411: System.Console.dll => 121
	i64 u0xc12b8b3afa48329c, ; 412: lib_System.Linq.dll.so => 134
	i64 u0xc1ff9ae3cdb6e1e6, ; 413: Xamarin.AndroidX.Activity.dll => 56
	i64 u0xc226d517e7e30388, ; 414: lib_Xamarin.Grpc.Stub.dll.so => 105
	i64 u0xc28c50f32f81cc73, ; 415: ja/Microsoft.Maui.Controls.resources.dll => 15
	i64 u0xc2bcfec99f69365e, ; 416: Xamarin.AndroidX.ViewPager2.dll => 83
	i64 u0xc421b61fd853169d, ; 417: lib_System.Net.WebSockets.Client.dll.so => 144
	i64 u0xc4d3858ed4d08512, ; 418: Xamarin.AndroidX.Lifecycle.ViewModelSavedState.dll => 71
	i64 u0xc50fded0ded1418c, ; 419: lib_System.ComponentModel.TypeConverter.dll.so => 119
	i64 u0xc519125d6bc8fb11, ; 420: lib_System.Net.Requests.dll.so => 140
	i64 u0xc5293b19e4dc230e, ; 421: Xamarin.AndroidX.Navigation.Fragment => 75
	i64 u0xc5325b2fcb37446f, ; 422: lib_System.Private.Xml.dll.so => 150
	i64 u0xc5a0f4b95a699af7, ; 423: lib_System.Private.Uri.dll.so => 148
	i64 u0xc5fe73d2394f68ac, ; 424: Xamarin.Io.OpenCensus.OpenCensusApi.dll => 106
	i64 u0xc62af3e2d6d38289, ; 425: lib_Xamarin.Firebase.Firestore.dll.so => 89
	i64 u0xc7c01e7d7c93a110, ; 426: System.Text.Encoding.Extensions.dll => 161
	i64 u0xc7ce851898a4548e, ; 427: lib_System.Web.HttpUtility.dll.so => 169
	i64 u0xc858a28d9ee5a6c5, ; 428: lib_System.Collections.Specialized.dll.so => 116
	i64 u0xca3a723e7342c5b6, ; 429: lib-tr-Microsoft.Maui.Controls.resources.dll.so => 28
	i64 u0xcab3493c70141c2d, ; 430: pl/Microsoft.Maui.Controls.resources => 20
	i64 u0xcacfddc9f7c6de76, ; 431: ro/Microsoft.Maui.Controls.resources.dll => 23
	i64 u0xcb76efab0f56f81a, ; 432: System.Reactive => 55
	i64 u0xcbd4fdd9cef4a294, ; 433: lib__Microsoft.Android.Resource.Designer.dll.so => 34
	i64 u0xcc2876b32ef2794c, ; 434: lib_System.Text.RegularExpressions.dll.so => 164
	i64 u0xcc5c3bb714c4561e, ; 435: Xamarin.KotlinX.Coroutines.Core.Jvm.dll => 109
	i64 u0xcc76886e09b88260, ; 436: Xamarin.KotlinX.Serialization.Core.Jvm.dll => 110
	i64 u0xccf25c4b634ccd3a, ; 437: zh-Hans/Microsoft.Maui.Controls.resources.dll => 32
	i64 u0xcd10a42808629144, ; 438: System.Net.Requests => 140
	i64 u0xcdd0c48b6937b21c, ; 439: Xamarin.AndroidX.SwipeRefreshLayout => 81
	i64 u0xcf23d8093f3ceadf, ; 440: System.Diagnostics.DiagnosticSource.dll => 124
	i64 u0xcf8fc898f98b0d34, ; 441: System.Private.Xml.Linq => 149
	i64 u0xd1194e1d8a8de83c, ; 442: lib_Xamarin.AndroidX.Lifecycle.Common.Jvm.dll.so => 68
	i64 u0xd20588bdafd1c17c, ; 443: Xamarin.Grpc.Stub.dll => 105
	i64 u0xd333d0af9e423810, ; 444: System.Runtime.InteropServices => 154
	i64 u0xd3426d966bb704f5, ; 445: Xamarin.AndroidX.AppCompat.AppCompatResources.dll => 58
	i64 u0xd3651b6fc3125825, ; 446: System.Private.Uri.dll => 148
	i64 u0xd373685349b1fe8b, ; 447: Microsoft.Extensions.Logging.dll => 39
	i64 u0xd3e4c8d6a2d5d470, ; 448: it/Microsoft.Maui.Controls.resources => 14
	i64 u0xd4645626dffec99d, ; 449: lib_Microsoft.Extensions.DependencyInjection.Abstractions.dll.so => 38
	i64 u0xd5507e11a2b2839f, ; 450: Xamarin.AndroidX.Lifecycle.ViewModelSavedState => 71
	i64 u0xd6694f8359737e4e, ; 451: Xamarin.AndroidX.SavedState => 79
	i64 u0xd6d21782156bc35b, ; 452: Xamarin.AndroidX.SwipeRefreshLayout.dll => 81
	i64 u0xd6ed09ee80649430, ; 453: lib_Xamarin.Grpc.Core.dll.so => 102
	i64 u0xd72329819cbbbc44, ; 454: lib_Microsoft.Extensions.Configuration.Abstractions.dll.so => 36
	i64 u0xd7b3764ada9d341d, ; 455: lib_Microsoft.Extensions.Logging.Abstractions.dll.so => 40
	i64 u0xd7dfd89d34e8dd1d, ; 456: Square.OkIO => 54
	i64 u0xda1dfa4c534a9251, ; 457: Microsoft.Extensions.DependencyInjection => 37
	i64 u0xdad05a11827959a3, ; 458: System.Collections.NonGeneric.dll => 115
	i64 u0xdaff1e02a729f3a2, ; 459: Xamarin.Grpc.Android => 99
	i64 u0xdb5383ab5865c007, ; 460: lib-vi-Microsoft.Maui.Controls.resources.dll.so => 30
	i64 u0xdb58816721c02a59, ; 461: lib_System.Reflection.Emit.ILGeneration.dll.so => 151
	i64 u0xdbeda89f832aa805, ; 462: vi/Microsoft.Maui.Controls.resources.dll => 30
	i64 u0xdbf9607a441b4505, ; 463: System.Linq => 134
	i64 u0xdca8be7403f92d4f, ; 464: lib_System.Linq.Queryable.dll.so => 133
	i64 u0xdce2c53525640bf3, ; 465: Microsoft.Extensions.Logging => 39
	i64 u0xdd2b722d78ef5f43, ; 466: System.Runtime.dll => 159
	i64 u0xdd67031857c72f96, ; 467: lib_System.Text.Encodings.Web.dll.so => 162
	i64 u0xdde30e6b77aa6f6c, ; 468: lib-zh-Hans-Microsoft.Maui.Controls.resources.dll.so => 32
	i64 u0xde110ae80fa7c2e2, ; 469: System.Xml.XDocument.dll => 172
	i64 u0xde8769ebda7d8647, ; 470: hr/Microsoft.Maui.Controls.resources.dll => 11
	i64 u0xe0142572c095a480, ; 471: Xamarin.AndroidX.AppCompat.dll => 57
	i64 u0xe02f89350ec78051, ; 472: Xamarin.AndroidX.CoordinatorLayout.dll => 62
	i64 u0xe192a588d4410686, ; 473: lib_System.IO.Pipelines.dll.so => 131
	i64 u0xe1a08bd3fa539e0d, ; 474: System.Runtime.Loader => 155
	i64 u0xe1b52f9f816c70ef, ; 475: System.Private.Xml.Linq.dll => 149
	i64 u0xe1ecfdb7fff86067, ; 476: System.Net.Security.dll => 141
	i64 u0xe2420585aeceb728, ; 477: System.Net.Requests.dll => 140
	i64 u0xe29b73bc11392966, ; 478: lib-id-Microsoft.Maui.Controls.resources.dll.so => 13
	i64 u0xe2a295b75d36df94, ; 479: Xamarin.Firebase.Auth.dll => 84
	i64 u0xe3811d68d4fe8463, ; 480: pt-BR/Microsoft.Maui.Controls.resources.dll => 21
	i64 u0xe494f7ced4ecd10a, ; 481: hu/Microsoft.Maui.Controls.resources.dll => 12
	i64 u0xe4a9b1e40d1e8917, ; 482: lib-fi-Microsoft.Maui.Controls.resources.dll.so => 7
	i64 u0xe4f74a0b5bf9703f, ; 483: System.Runtime.Serialization.Primitives => 158
	i64 u0xe5434e8a119ceb69, ; 484: lib_Mono.Android.dll.so => 178
	i64 u0xe89a2a9ef110899b, ; 485: System.Drawing.dll => 127
	i64 u0xe8efe6c2171f7cd2, ; 486: Xamarin.Google.Guava.dll => 94
	i64 u0xe93ca41931f1f2d0, ; 487: Xamarin.Grpc.Api.dll => 100
	i64 u0xeb6e275e78cb8d42, ; 488: Xamarin.AndroidX.LocalBroadcastManager.dll => 73
	i64 u0xeb9973cda26e858f, ; 489: Xamarin.Firebase.Common.dll => 86
	i64 u0xedc4817167106c23, ; 490: System.Net.Sockets.dll => 142
	i64 u0xedc632067fb20ff3, ; 491: System.Memory.dll => 135
	i64 u0xedc8e4ca71a02a8b, ; 492: Xamarin.AndroidX.Navigation.Runtime.dll => 76
	i64 u0xee81f5b3f1c4f83b, ; 493: System.Threading.ThreadPool => 167
	i64 u0xeeb7ebb80150501b, ; 494: lib_Xamarin.AndroidX.Collection.Jvm.dll.so => 61
	i64 u0xef72742e1bcca27a, ; 495: Microsoft.Maui.Essentials.dll => 46
	i64 u0xefec0b7fdc57ec42, ; 496: Xamarin.AndroidX.Activity => 56
	i64 u0xf00c29406ea45e19, ; 497: es/Microsoft.Maui.Controls.resources.dll => 6
	i64 u0xf09e47b6ae914f6e, ; 498: System.Net.NameResolution => 137
	i64 u0xf0de2537ee19c6ca, ; 499: lib_System.Net.WebHeaderCollection.dll.so => 143
	i64 u0xf11b621fc87b983f, ; 500: Microsoft.Maui.Controls.Xaml.dll => 44
	i64 u0xf1c4b4005493d871, ; 501: System.Formats.Asn1.dll => 128
	i64 u0xf238bd79489d3a96, ; 502: lib-nl-Microsoft.Maui.Controls.resources.dll.so => 19
	i64 u0xf37221fda4ef8830, ; 503: lib_Xamarin.Google.Android.Material.dll.so => 91
	i64 u0xf3ddfe05336abf29, ; 504: System => 173
	i64 u0xf408654b2a135055, ; 505: System.Reflection.Emit.ILGeneration.dll => 151
	i64 u0xf4103170a1de5bd0, ; 506: System.Linq.Queryable.dll => 133
	i64 u0xf483be3bba89b4ff, ; 507: lib_Xamarin.Grpc.Api.dll.so => 100
	i64 u0xf4c1dd70a5496a17, ; 508: System.IO.Compression => 130
	i64 u0xf5fc7602fe27b333, ; 509: System.Net.WebHeaderCollection => 143
	i64 u0xf6077741019d7428, ; 510: Xamarin.AndroidX.CoordinatorLayout => 62
	i64 u0xf73b506af603bfe1, ; 511: lib_Square.OkIO.dll.so => 54
	i64 u0xf77b20923f07c667, ; 512: de/Microsoft.Maui.Controls.resources.dll => 4
	i64 u0xf7e2cac4c45067b3, ; 513: lib_System.Numerics.Vectors.dll.so => 146
	i64 u0xf7e74930e0e3d214, ; 514: zh-HK/Microsoft.Maui.Controls.resources.dll => 31
	i64 u0xf7fa0bf77fe677cc, ; 515: Newtonsoft.Json.dll => 48
	i64 u0xf84773b5c81e3cef, ; 516: lib-uk-Microsoft.Maui.Controls.resources.dll.so => 29
	i64 u0xf8aac5ea82de1348, ; 517: System.Linq.Queryable => 133
	i64 u0xf8b77539b362d3ba, ; 518: lib_System.Reflection.Primitives.dll.so => 153
	i64 u0xf8dacc6dd9573437, ; 519: Square.OkIO.dll => 54
	i64 u0xf8e045dc345b2ea3, ; 520: lib_Xamarin.AndroidX.RecyclerView.dll.so => 78
	i64 u0xf915dc29808193a1, ; 521: System.Web.HttpUtility.dll => 169
	i64 u0xf96c777a2a0686f4, ; 522: hi/Microsoft.Maui.Controls.resources.dll => 10
	i64 u0xf9eec5bb3a6aedc6, ; 523: Microsoft.Extensions.Options => 41
	i64 u0xfa3f278f288b0e84, ; 524: lib_System.Net.Security.dll.so => 141
	i64 u0xfa5ed7226d978949, ; 525: lib-ar-Microsoft.Maui.Controls.resources.dll.so => 0
	i64 u0xfa645d91e9fc4cba, ; 526: System.Threading.Thread => 166
	i64 u0xfbf0a31c9fc34bc4, ; 527: lib_System.Net.Http.dll.so => 136
	i64 u0xfc61ddcf78dd1f54, ; 528: Xamarin.AndroidX.LocalBroadcastManager => 73
	i64 u0xfc6b7527cc280b3f, ; 529: lib_System.Runtime.Serialization.Formatters.dll.so => 157
	i64 u0xfc719aec26adf9d9, ; 530: Xamarin.AndroidX.Navigation.Fragment.dll => 75
	i64 u0xfd22f00870e40ae0, ; 531: lib_Xamarin.AndroidX.DrawerLayout.dll.so => 66
	i64 u0xfd3ce7bc9232d417, ; 532: Xamarin.Firebase.Firestore.dll => 89
	i64 u0xfd536c702f64dc47, ; 533: System.Text.Encoding.Extensions => 161
	i64 u0xfd583f7657b6a1cb, ; 534: Xamarin.AndroidX.Fragment => 67
	i64 u0xfda36abccf05cf5c, ; 535: System.Net.WebSockets.Client => 144
	i64 u0xfeae9952cf03b8cb ; 536: tr/Microsoft.Maui.Controls.resources => 28
], align 16

@assembly_image_cache_indices = dso_local local_unnamed_addr constant [537 x i32] [
	i32 81, i32 76, i32 177, i32 57, i32 24, i32 2, i32 30, i32 101,
	i32 139, i32 98, i32 78, i32 117, i32 45, i32 31, i32 170, i32 61,
	i32 174, i32 24, i32 115, i32 153, i32 66, i32 41, i32 115, i32 97,
	i32 160, i32 87, i32 165, i32 25, i32 110, i32 82, i32 21, i32 178,
	i32 46, i32 80, i32 137, i32 65, i32 52, i32 129, i32 145, i32 112,
	i32 152, i32 78, i32 8, i32 176, i32 9, i32 38, i32 145, i32 105,
	i32 106, i32 122, i32 12, i32 162, i32 110, i32 18, i32 114, i32 173,
	i32 27, i32 177, i32 80, i32 95, i32 77, i32 16, i32 41, i32 129,
	i32 159, i32 104, i32 151, i32 27, i32 97, i32 166, i32 121, i32 63,
	i32 158, i32 88, i32 8, i32 93, i32 108, i32 42, i32 89, i32 13,
	i32 11, i32 145, i32 93, i32 176, i32 99, i32 139, i32 29, i32 138,
	i32 125, i32 7, i32 164, i32 128, i32 106, i32 33, i32 20, i32 152,
	i32 149, i32 168, i32 95, i32 26, i32 88, i32 163, i32 5, i32 49,
	i32 171, i32 88, i32 67, i32 34, i32 59, i32 126, i32 8, i32 171,
	i32 6, i32 142, i32 102, i32 45, i32 2, i32 111, i32 43, i32 84,
	i32 83, i32 35, i32 104, i32 87, i32 152, i32 51, i32 153, i32 65,
	i32 137, i32 82, i32 55, i32 1, i32 55, i32 48, i32 52, i32 161,
	i32 108, i32 103, i32 85, i32 167, i32 170, i32 63, i32 74, i32 174,
	i32 58, i32 178, i32 20, i32 113, i32 158, i32 108, i32 125, i32 24,
	i32 107, i32 170, i32 51, i32 22, i32 147, i32 86, i32 111, i32 77,
	i32 163, i32 72, i32 138, i32 144, i32 132, i32 150, i32 155, i32 14,
	i32 72, i32 177, i32 165, i32 1, i32 99, i32 43, i32 92, i32 70,
	i32 85, i32 127, i32 139, i32 123, i32 63, i32 47, i32 25, i32 138,
	i32 31, i32 73, i32 159, i32 68, i32 116, i32 148, i32 95, i32 175,
	i32 124, i32 15, i32 37, i32 113, i32 103, i32 62, i32 168, i32 120,
	i32 94, i32 3, i32 96, i32 60, i32 39, i32 143, i32 154, i32 61,
	i32 116, i32 162, i32 118, i32 171, i32 123, i32 5, i32 49, i32 37,
	i32 109, i32 135, i32 44, i32 4, i32 155, i32 175, i32 85, i32 94,
	i32 91, i32 43, i32 156, i32 121, i32 70, i32 64, i32 3, i32 126,
	i32 128, i32 9, i32 154, i32 18, i32 122, i32 111, i32 47, i32 42,
	i32 64, i32 42, i32 75, i32 122, i32 45, i32 2, i32 102, i32 28,
	i32 50, i32 18, i32 14, i32 118, i32 11, i32 135, i32 35, i32 79,
	i32 156, i32 92, i32 107, i32 17, i32 27, i32 86, i32 67, i32 53,
	i32 60, i32 7, i32 119, i32 25, i32 4, i32 92, i32 97, i32 17,
	i32 146, i32 117, i32 103, i32 147, i32 98, i32 120, i32 82, i32 36,
	i32 69, i32 173, i32 33, i32 57, i32 59, i32 127, i32 29, i32 32,
	i32 60, i32 101, i32 50, i32 33, i32 35, i32 104, i32 90, i32 166,
	i32 129, i32 46, i32 109, i32 118, i32 72, i32 124, i32 9, i32 90,
	i32 64, i32 168, i32 114, i32 53, i32 48, i32 74, i32 10, i32 23,
	i32 98, i32 22, i32 21, i32 101, i32 125, i32 34, i32 130, i32 70,
	i32 44, i32 84, i32 65, i32 163, i32 134, i32 1, i32 87, i32 17,
	i32 130, i32 6, i32 13, i32 47, i32 96, i32 120, i32 114, i32 132,
	i32 76, i32 16, i32 50, i32 56, i32 36, i32 19, i32 74, i32 69,
	i32 107, i32 160, i32 93, i32 91, i32 77, i32 131, i32 174, i32 16,
	i32 123, i32 157, i32 146, i32 160, i32 79, i32 80, i32 66, i32 68,
	i32 12, i32 96, i32 40, i32 150, i32 136, i32 38, i32 5, i32 132,
	i32 156, i32 69, i32 172, i32 23, i32 165, i32 19, i32 52, i32 169,
	i32 119, i32 141, i32 176, i32 147, i32 51, i32 71, i32 26, i32 164,
	i32 3, i32 59, i32 90, i32 10, i32 0, i32 131, i32 40, i32 167,
	i32 126, i32 26, i32 175, i32 113, i32 22, i32 15, i32 172, i32 117,
	i32 112, i32 142, i32 157, i32 100, i32 112, i32 136, i32 83, i32 53,
	i32 58, i32 0, i32 49, i32 121, i32 134, i32 56, i32 105, i32 15,
	i32 83, i32 144, i32 71, i32 119, i32 140, i32 75, i32 150, i32 148,
	i32 106, i32 89, i32 161, i32 169, i32 116, i32 28, i32 20, i32 23,
	i32 55, i32 34, i32 164, i32 109, i32 110, i32 32, i32 140, i32 81,
	i32 124, i32 149, i32 68, i32 105, i32 154, i32 58, i32 148, i32 39,
	i32 14, i32 38, i32 71, i32 79, i32 81, i32 102, i32 36, i32 40,
	i32 54, i32 37, i32 115, i32 99, i32 30, i32 151, i32 30, i32 134,
	i32 133, i32 39, i32 159, i32 162, i32 32, i32 172, i32 11, i32 57,
	i32 62, i32 131, i32 155, i32 149, i32 141, i32 140, i32 13, i32 84,
	i32 21, i32 12, i32 7, i32 158, i32 178, i32 127, i32 94, i32 100,
	i32 73, i32 86, i32 142, i32 135, i32 76, i32 167, i32 61, i32 46,
	i32 56, i32 6, i32 137, i32 143, i32 44, i32 128, i32 19, i32 91,
	i32 173, i32 151, i32 133, i32 100, i32 130, i32 143, i32 62, i32 54,
	i32 4, i32 146, i32 31, i32 48, i32 29, i32 133, i32 153, i32 54,
	i32 78, i32 169, i32 10, i32 41, i32 141, i32 0, i32 166, i32 136,
	i32 73, i32 157, i32 75, i32 66, i32 89, i32 161, i32 67, i32 144,
	i32 28
], align 16

@marshal_methods_number_of_classes = dso_local local_unnamed_addr constant i32 0, align 4

@marshal_methods_class_cache = dso_local local_unnamed_addr global [0 x %struct.MarshalMethodsManagedClass] zeroinitializer, align 8

; Names of classes in which marshal methods reside
@mm_class_names = dso_local local_unnamed_addr constant [0 x ptr] zeroinitializer, align 8

@mm_method_names = dso_local local_unnamed_addr constant [1 x %struct.MarshalMethodName] [
	%struct.MarshalMethodName {
		i64 u0x0000000000000000, ; name: 
		ptr @.MarshalMethodName.0_name; char* name
	} ; 0
], align 8

; get_function_pointer (uint32_t mono_image_index, uint32_t class_index, uint32_t method_token, void*& target_ptr)
@get_function_pointer = internal dso_local unnamed_addr global ptr null, align 8

; Functions

; Function attributes: memory(write, argmem: none, inaccessiblemem: none) "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" nofree norecurse nosync nounwind "stack-protector-buffer-size"="8" uwtable willreturn
define void @xamarin_app_init(ptr nocapture noundef readnone %env, ptr noundef %fn) local_unnamed_addr #0
{
	%fnIsNull = icmp eq ptr %fn, null
	br i1 %fnIsNull, label %1, label %2

1: ; preds = %0
	%putsResult = call noundef i32 @puts(ptr @.str.0)
	call void @abort()
	unreachable 

2: ; preds = %1, %0
	store ptr %fn, ptr @get_function_pointer, align 8, !tbaa !3
	ret void
}

; Strings
@.str.0 = private unnamed_addr constant [40 x i8] c"get_function_pointer MUST be specified\0A\00", align 16

;MarshalMethodName
@.MarshalMethodName.0_name = private unnamed_addr constant [1 x i8] c"\00", align 1

; External functions

; Function attributes: "no-trapping-math"="true" noreturn nounwind "stack-protector-buffer-size"="8"
declare void @abort() local_unnamed_addr #2

; Function attributes: nofree nounwind
declare noundef i32 @puts(ptr noundef) local_unnamed_addr #1
attributes #0 = { memory(write, argmem: none, inaccessiblemem: none) "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" nofree norecurse nosync nounwind "stack-protector-buffer-size"="8" "target-cpu"="x86-64" "target-features"="+crc32,+cx16,+cx8,+fxsr,+mmx,+popcnt,+sse,+sse2,+sse3,+sse4.1,+sse4.2,+ssse3,+x87" "tune-cpu"="generic" uwtable willreturn }
attributes #1 = { nofree nounwind }
attributes #2 = { "no-trapping-math"="true" noreturn nounwind "stack-protector-buffer-size"="8" "target-cpu"="x86-64" "target-features"="+crc32,+cx16,+cx8,+fxsr,+mmx,+popcnt,+sse,+sse2,+sse3,+sse4.1,+sse4.2,+ssse3,+x87" "tune-cpu"="generic" }

; Metadata
!llvm.module.flags = !{!0, !1}
!0 = !{i32 1, !"wchar_size", i32 4}
!1 = !{i32 7, !"PIC Level", i32 2}
!llvm.ident = !{!2}
!2 = !{!".NET for Android remotes/origin/release/9.0.1xx @ 1dcfb6f8779c33b6f768c996495cb90ecd729329"}
!3 = !{!4, !4, i64 0}
!4 = !{!"any pointer", !5, i64 0}
!5 = !{!"omnipotent char", !6, i64 0}
!6 = !{!"Simple C++ TBAA"}
