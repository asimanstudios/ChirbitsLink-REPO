; ModuleID = 'marshal_methods.x86.ll'
source_filename = "marshal_methods.x86.ll"
target datalayout = "e-m:e-p:32:32-p270:32:32-p271:32:32-p272:64:64-f64:32:64-f80:32-n8:16:32-S128"
target triple = "i686-unknown-linux-android21"

%struct.MarshalMethodName = type {
	i64, ; uint64_t id
	ptr ; char* name
}

%struct.MarshalMethodsManagedClass = type {
	i32, ; uint32_t token
	ptr ; MonoClass klass
}

@assembly_image_cache = dso_local local_unnamed_addr global [169 x ptr] zeroinitializer, align 4

; Each entry maps hash of an assembly name to an index into the `assembly_image_cache` array
@assembly_image_cache_hashes = dso_local local_unnamed_addr constant [338 x i32] [
	i32 2616222, ; 0: System.Net.NetworkInformation.dll => 0x27eb9e => 129
	i32 6657927, ; 1: Xamarin.Grpc.Protobuf.Lite.dll => 0x659787 => 97
	i32 9414545, ; 2: Xamarin.Grpc.Android => 0x8fa791 => 92
	i32 10166715, ; 3: System.Net.NameResolution.dll => 0x9b21bb => 128
	i32 39109920, ; 4: Newtonsoft.Json.dll => 0x254c520 => 48
	i32 39485524, ; 5: System.Net.WebSockets.dll => 0x25a8054 => 136
	i32 42639949, ; 6: System.Threading.Thread => 0x28aa24d => 157
	i32 67008169, ; 7: zh-Hant\Microsoft.Maui.Controls.resources => 0x3fe76a9 => 33
	i32 72070932, ; 8: Microsoft.Maui.Graphics.dll => 0x44bb714 => 47
	i32 117431740, ; 9: System.Runtime.InteropServices => 0x6ffddbc => 145
	i32 122350210, ; 10: System.Threading.Channels.dll => 0x74aea82 => 156
	i32 142721839, ; 11: System.Net.WebHeaderCollection => 0x881c32f => 134
	i32 165246403, ; 12: Xamarin.AndroidX.Collection.dll => 0x9d975c3 => 60
	i32 182336117, ; 13: Xamarin.AndroidX.SwipeRefreshLayout.dll => 0xade3a75 => 79
	i32 195452805, ; 14: vi/Microsoft.Maui.Controls.resources.dll => 0xba65f85 => 30
	i32 199333315, ; 15: zh-HK/Microsoft.Maui.Controls.resources.dll => 0xbe195c3 => 31
	i32 205061960, ; 16: System.ComponentModel => 0xc38ff48 => 112
	i32 230752869, ; 17: Microsoft.CSharp.dll => 0xdc10265 => 105
	i32 246610117, ; 18: System.Reflection.Emit.Lightweight => 0xeb2f8c5 => 143
	i32 271099684, ; 19: Xamarin.Grpc.OkHttp => 0x1028a724 => 96
	i32 273568582, ; 20: Plugin.BLE => 0x104e5346 => 49
	i32 280992041, ; 21: cs/Microsoft.Maui.Controls.resources.dll => 0x10bf9929 => 2
	i32 317674968, ; 22: vi\Microsoft.Maui.Controls.resources => 0x12ef55d8 => 30
	i32 318968648, ; 23: Xamarin.AndroidX.Activity.dll => 0x13031348 => 56
	i32 336156722, ; 24: ja/Microsoft.Maui.Controls.resources.dll => 0x14095832 => 15
	i32 342366114, ; 25: Xamarin.AndroidX.Lifecycle.Common => 0x146817a2 => 68
	i32 356389973, ; 26: it/Microsoft.Maui.Controls.resources.dll => 0x153e1455 => 14
	i32 379916513, ; 27: System.Threading.Thread.dll => 0x16a510e1 => 157
	i32 385762202, ; 28: System.Memory.dll => 0x16fe439a => 126
	i32 392610295, ; 29: System.Threading.ThreadPool.dll => 0x1766c1f7 => 158
	i32 395744057, ; 30: _Microsoft.Android.Resource.Designer => 0x17969339 => 34
	i32 419244643, ; 31: Plugin.FirebaseAuth.dll => 0x18fd2a63 => 52
	i32 435591531, ; 32: sv/Microsoft.Maui.Controls.resources.dll => 0x19f6996b => 26
	i32 442565967, ; 33: System.Collections => 0x1a61054f => 109
	i32 450948140, ; 34: Xamarin.AndroidX.Fragment.dll => 0x1ae0ec2c => 67
	i32 453011810, ; 35: Xamarin.Firebase.Database.Collection.dll => 0x1b006962 => 86
	i32 459347974, ; 36: System.Runtime.Serialization.Primitives.dll => 0x1b611806 => 149
	i32 465846621, ; 37: mscorlib => 0x1bc4415d => 164
	i32 469710990, ; 38: System.dll => 0x1bff388e => 163
	i32 498788369, ; 39: System.ObjectModel => 0x1dbae811 => 138
	i32 500358224, ; 40: id/Microsoft.Maui.Controls.resources.dll => 0x1dd2dc50 => 13
	i32 503918385, ; 41: fi/Microsoft.Maui.Controls.resources.dll => 0x1e092f31 => 7
	i32 513247710, ; 42: Microsoft.Extensions.Primitives.dll => 0x1e9789de => 42
	i32 530272170, ; 43: System.Linq.Queryable => 0x1f9b4faa => 124
	i32 539058512, ; 44: Microsoft.Extensions.Logging => 0x20216150 => 39
	i32 592146354, ; 45: pt-BR/Microsoft.Maui.Controls.resources.dll => 0x234b6fb2 => 21
	i32 610194910, ; 46: System.Reactive.dll => 0x245ed5de => 55
	i32 627609679, ; 47: Xamarin.AndroidX.CustomView => 0x2568904f => 65
	i32 627931235, ; 48: nl\Microsoft.Maui.Controls.resources => 0x256d7863 => 19
	i32 662205335, ; 49: System.Text.Encodings.Web.dll => 0x27787397 => 153
	i32 672442732, ; 50: System.Collections.Concurrent => 0x2814a96c => 106
	i32 683518922, ; 51: System.Net.Security => 0x28bdabca => 132
	i32 688181140, ; 52: ca/Microsoft.Maui.Controls.resources.dll => 0x2904cf94 => 1
	i32 690569205, ; 53: System.Xml.Linq.dll => 0x29293ff5 => 160
	i32 706645707, ; 54: ko/Microsoft.Maui.Controls.resources.dll => 0x2a1e8ecb => 16
	i32 709557578, ; 55: de/Microsoft.Maui.Controls.resources.dll => 0x2a4afd4a => 4
	i32 712915335, ; 56: Xamarin.Grpc.Api => 0x2a7e3987 => 93
	i32 722857257, ; 57: System.Runtime.Loader.dll => 0x2b15ed29 => 146
	i32 759454413, ; 58: System.Net.Requests => 0x2d445acd => 131
	i32 775507847, ; 59: System.IO.Compression => 0x2e394f87 => 122
	i32 777317022, ; 60: sk\Microsoft.Maui.Controls.resources => 0x2e54ea9e => 25
	i32 789151979, ; 61: Microsoft.Extensions.Options => 0x2f0980eb => 41
	i32 804715423, ; 62: System.Data.Common => 0x2ff6fb9f => 115
	i32 823281589, ; 63: System.Private.Uri.dll => 0x311247b5 => 139
	i32 830298997, ; 64: System.IO.Compression.Brotli => 0x317d5b75 => 121
	i32 864477724, ; 65: Plugin.BLE.dll => 0x3386e21c => 49
	i32 884236112, ; 66: Plugin.CloudFirestore.dll => 0x34b45f50 => 50
	i32 904024072, ; 67: System.ComponentModel.Primitives.dll => 0x35e25008 => 110
	i32 926902833, ; 68: tr/Microsoft.Maui.Controls.resources.dll => 0x373f6a31 => 28
	i32 955402788, ; 69: Newtonsoft.Json => 0x38f24a24 => 48
	i32 957807352, ; 70: Plugin.CurrentActivity => 0x3916faf8 => 51
	i32 961995525, ; 71: Square.OkIO.dll => 0x3956e305 => 54
	i32 967690846, ; 72: Xamarin.AndroidX.Lifecycle.Common.dll => 0x39adca5e => 68
	i32 975874589, ; 73: System.Xml.XDocument => 0x3a2aaa1d => 162
	i32 992768348, ; 74: System.Collections.dll => 0x3b2c715c => 109
	i32 1012816738, ; 75: Xamarin.AndroidX.SavedState.dll => 0x3c5e5b62 => 78
	i32 1019214401, ; 76: System.Drawing => 0x3cbffa41 => 119
	i32 1028951442, ; 77: Microsoft.Extensions.DependencyInjection.Abstractions => 0x3d548d92 => 38
	i32 1029334545, ; 78: da/Microsoft.Maui.Controls.resources.dll => 0x3d5a6611 => 3
	i32 1035644815, ; 79: Xamarin.AndroidX.AppCompat => 0x3dbaaf8f => 57
	i32 1036536393, ; 80: System.Drawing.Primitives.dll => 0x3dc84a49 => 118
	i32 1044663988, ; 81: System.Linq.Expressions.dll => 0x3e444eb4 => 123
	i32 1050026713, ; 82: Xamarin.Io.OpenCensus.OpenCensusApi.dll => 0x3e9622d9 => 99
	i32 1052210849, ; 83: Xamarin.AndroidX.Lifecycle.ViewModel.dll => 0x3eb776a1 => 70
	i32 1082857460, ; 84: System.ComponentModel.TypeConverter => 0x408b17f4 => 111
	i32 1084122840, ; 85: Xamarin.Kotlin.StdLib => 0x409e66d8 => 101
	i32 1098259244, ; 86: System => 0x41761b2c => 163
	i32 1110581358, ; 87: Xamarin.Firebase.Auth => 0x4232206e => 82
	i32 1118262833, ; 88: ko\Microsoft.Maui.Controls.resources => 0x42a75631 => 16
	i32 1159499262, ; 89: Xamarin.Grpc.Stub.dll => 0x451c8dfe => 98
	i32 1168523401, ; 90: pt\Microsoft.Maui.Controls.resources => 0x45a64089 => 22
	i32 1178241025, ; 91: Xamarin.AndroidX.Navigation.Runtime.dll => 0x463a8801 => 75
	i32 1203215381, ; 92: pl/Microsoft.Maui.Controls.resources.dll => 0x47b79c15 => 20
	i32 1208648034, ; 93: Square.OkHttp => 0x480a8162 => 53
	i32 1230765884, ; 94: Xamarin.Grpc.Stub => 0x495bff3c => 98
	i32 1234928153, ; 95: nb/Microsoft.Maui.Controls.resources.dll => 0x499b8219 => 18
	i32 1244346141, ; 96: Xamarin.Protobuf.Lite => 0x4a2b371d => 103
	i32 1246548578, ; 97: Xamarin.AndroidX.Collection.Jvm.dll => 0x4a4cd262 => 61
	i32 1260983243, ; 98: cs\Microsoft.Maui.Controls.resources => 0x4b2913cb => 2
	i32 1273391546, ; 99: Xamarin.Io.OpenCensus.OpenCensusContribGrpcMetrics => 0x4be669ba => 100
	i32 1293217323, ; 100: Xamarin.AndroidX.DrawerLayout.dll => 0x4d14ee2b => 66
	i32 1324164729, ; 101: System.Linq => 0x4eed2679 => 125
	i32 1333047053, ; 102: Xamarin.Firebase.Common => 0x4f74af0d => 84
	i32 1373134921, ; 103: zh-Hans\Microsoft.Maui.Controls.resources => 0x51d86049 => 32
	i32 1376866003, ; 104: Xamarin.AndroidX.SavedState => 0x52114ed3 => 78
	i32 1406073936, ; 105: Xamarin.AndroidX.CoordinatorLayout => 0x53cefc50 => 62
	i32 1408764838, ; 106: System.Runtime.Serialization.Formatters.dll => 0x53f80ba6 => 148
	i32 1411702249, ; 107: Xamarin.Firebase.Auth.Interop.dll => 0x5424dde9 => 83
	i32 1430672901, ; 108: ar\Microsoft.Maui.Controls.resources => 0x55465605 => 0
	i32 1452070440, ; 109: System.Formats.Asn1.dll => 0x568cd628 => 120
	i32 1458022317, ; 110: System.Net.Security.dll => 0x56e7a7ad => 132
	i32 1461004990, ; 111: es\Microsoft.Maui.Controls.resources => 0x57152abe => 6
	i32 1462112819, ; 112: System.IO.Compression.dll => 0x57261233 => 122
	i32 1469204771, ; 113: Xamarin.AndroidX.AppCompat.AppCompatResources => 0x57924923 => 58
	i32 1470490898, ; 114: Microsoft.Extensions.Primitives => 0x57a5e912 => 42
	i32 1480492111, ; 115: System.IO.Compression.Brotli.dll => 0x583e844f => 121
	i32 1493001747, ; 116: hi/Microsoft.Maui.Controls.resources.dll => 0x58fd6613 => 10
	i32 1514721132, ; 117: el/Microsoft.Maui.Controls.resources.dll => 0x5a48cf6c => 5
	i32 1543031311, ; 118: System.Text.RegularExpressions.dll => 0x5bf8ca0f => 155
	i32 1544135863, ; 119: Xamarin.Grpc.Api.dll => 0x5c09a4b7 => 93
	i32 1551623176, ; 120: sk/Microsoft.Maui.Controls.resources.dll => 0x5c7be408 => 25
	i32 1618516317, ; 121: System.Net.WebSockets.Client.dll => 0x6078995d => 135
	i32 1622152042, ; 122: Xamarin.AndroidX.Loader.dll => 0x60b0136a => 72
	i32 1624863272, ; 123: Xamarin.AndroidX.ViewPager2 => 0x60d97228 => 81
	i32 1636350590, ; 124: Xamarin.AndroidX.CursorAdapter => 0x6188ba7e => 64
	i32 1639515021, ; 125: System.Net.Http.dll => 0x61b9038d => 127
	i32 1639986890, ; 126: System.Text.RegularExpressions => 0x61c036ca => 155
	i32 1657153582, ; 127: System.Runtime => 0x62c6282e => 150
	i32 1658251792, ; 128: Xamarin.Google.Android.Material.dll => 0x62d6ea10 => 89
	i32 1664238415, ; 129: Xamarin.Firebase.Database.Collection => 0x6332434f => 86
	i32 1677501392, ; 130: System.Net.Primitives.dll => 0x63fca3d0 => 130
	i32 1678508291, ; 131: System.Net.WebSockets => 0x640c0103 => 136
	i32 1679769178, ; 132: System.Security.Cryptography => 0x641f3e5a => 151
	i32 1729485958, ; 133: Xamarin.AndroidX.CardView.dll => 0x6715dc86 => 59
	i32 1736233607, ; 134: ro/Microsoft.Maui.Controls.resources.dll => 0x677cd287 => 23
	i32 1743415430, ; 135: ca\Microsoft.Maui.Controls.resources => 0x67ea6886 => 1
	i32 1763938596, ; 136: System.Diagnostics.TraceSource.dll => 0x69239124 => 117
	i32 1766324549, ; 137: Xamarin.AndroidX.SwipeRefreshLayout => 0x6947f945 => 79
	i32 1770582343, ; 138: Microsoft.Extensions.Logging.dll => 0x6988f147 => 39
	i32 1776026572, ; 139: System.Core.dll => 0x69dc03cc => 114
	i32 1780572499, ; 140: Mono.Android.Runtime.dll => 0x6a216153 => 167
	i32 1782862114, ; 141: ms\Microsoft.Maui.Controls.resources => 0x6a445122 => 17
	i32 1785684415, ; 142: Xamarin.Io.OpenCensus.OpenCensusContribGrpcMetrics.dll => 0x6a6f61bf => 100
	i32 1788241197, ; 143: Xamarin.AndroidX.Fragment => 0x6a96652d => 67
	i32 1793755602, ; 144: he\Microsoft.Maui.Controls.resources => 0x6aea89d2 => 9
	i32 1808609942, ; 145: Xamarin.AndroidX.Loader => 0x6bcd3296 => 72
	i32 1813058853, ; 146: Xamarin.Kotlin.StdLib.dll => 0x6c111525 => 101
	i32 1813201214, ; 147: Xamarin.Google.Android.Material => 0x6c13413e => 89
	i32 1818569960, ; 148: Xamarin.AndroidX.Navigation.UI.dll => 0x6c652ce8 => 76
	i32 1824175904, ; 149: System.Text.Encoding.Extensions => 0x6cbab720 => 152
	i32 1824722060, ; 150: System.Runtime.Serialization.Formatters => 0x6cc30c8c => 148
	i32 1828688058, ; 151: Microsoft.Extensions.Logging.Abstractions.dll => 0x6cff90ba => 40
	i32 1842015223, ; 152: uk/Microsoft.Maui.Controls.resources.dll => 0x6dcaebf7 => 29
	i32 1853025655, ; 153: sv\Microsoft.Maui.Controls.resources => 0x6e72ed77 => 26
	i32 1858542181, ; 154: System.Linq.Expressions => 0x6ec71a65 => 123
	i32 1870277092, ; 155: System.Reflection.Primitives => 0x6f7a29e4 => 144
	i32 1875053220, ; 156: Xamarin.Firebase.Auth.Interop => 0x6fc30aa4 => 83
	i32 1875935024, ; 157: fr\Microsoft.Maui.Controls.resources => 0x6fd07f30 => 8
	i32 1908813208, ; 158: Xamarin.GooglePlayServices.Basement => 0x71c62d98 => 90
	i32 1910275211, ; 159: System.Collections.NonGeneric.dll => 0x71dc7c8b => 107
	i32 1939592360, ; 160: System.Private.Xml.Linq => 0x739bd4a8 => 140
	i32 1968388702, ; 161: Microsoft.Extensions.Configuration.dll => 0x75533a5e => 35
	i32 2003115576, ; 162: el\Microsoft.Maui.Controls.resources => 0x77651e38 => 5
	i32 2019465201, ; 163: Xamarin.AndroidX.Lifecycle.ViewModel => 0x785e97f1 => 70
	i32 2025202353, ; 164: ar/Microsoft.Maui.Controls.resources.dll => 0x78b622b1 => 0
	i32 2045470958, ; 165: System.Private.Xml => 0x79eb68ee => 141
	i32 2055257422, ; 166: Xamarin.AndroidX.Lifecycle.LiveData.Core.dll => 0x7a80bd4e => 69
	i32 2066184531, ; 167: de\Microsoft.Maui.Controls.resources => 0x7b277953 => 4
	i32 2070888862, ; 168: System.Diagnostics.TraceSource => 0x7b6f419e => 117
	i32 2079903147, ; 169: System.Runtime.dll => 0x7bf8cdab => 150
	i32 2083657273, ; 170: Xamarin.Firebase.ProtoliteWellKnownTypes => 0x7c321639 => 88
	i32 2090596640, ; 171: System.Numerics.Vectors => 0x7c9bf920 => 137
	i32 2127167465, ; 172: System.Console => 0x7ec9ffe9 => 113
	i32 2142473426, ; 173: System.Collections.Specialized => 0x7fb38cd2 => 108
	i32 2159891885, ; 174: Microsoft.Maui => 0x80bd55ad => 45
	i32 2169148018, ; 175: hu\Microsoft.Maui.Controls.resources => 0x814a9272 => 12
	i32 2181898931, ; 176: Microsoft.Extensions.Options.dll => 0x820d22b3 => 41
	i32 2192057212, ; 177: Microsoft.Extensions.Logging.Abstractions => 0x82a8237c => 40
	i32 2193016926, ; 178: System.ObjectModel.dll => 0x82b6c85e => 138
	i32 2195564014, ; 179: Xamarin.Grpc.Context => 0x82dda5ee => 94
	i32 2201107256, ; 180: Xamarin.KotlinX.Coroutines.Core.Jvm.dll => 0x83323b38 => 102
	i32 2201231467, ; 181: System.Net.Http => 0x8334206b => 127
	i32 2207618523, ; 182: it\Microsoft.Maui.Controls.resources => 0x839595db => 14
	i32 2266799131, ; 183: Microsoft.Extensions.Configuration.Abstractions => 0x871c9c1b => 36
	i32 2270573516, ; 184: fr/Microsoft.Maui.Controls.resources.dll => 0x875633cc => 8
	i32 2279755925, ; 185: Xamarin.AndroidX.RecyclerView.dll => 0x87e25095 => 77
	i32 2295906218, ; 186: System.Net.Sockets => 0x88d8bfaa => 133
	i32 2303942373, ; 187: nb\Microsoft.Maui.Controls.resources => 0x89535ee5 => 18
	i32 2305521784, ; 188: System.Private.CoreLib.dll => 0x896b7878 => 165
	i32 2307769286, ; 189: Square.OkHttp.dll => 0x898dc3c6 => 53
	i32 2353062107, ; 190: System.Net.Primitives => 0x8c40e0db => 130
	i32 2368005991, ; 191: System.Xml.ReaderWriter.dll => 0x8d24e767 => 161
	i32 2371007202, ; 192: Microsoft.Extensions.Configuration => 0x8d52b2e2 => 35
	i32 2382033717, ; 193: Xamarin.Firebase.Auth.dll => 0x8dfaf335 => 82
	i32 2395872292, ; 194: id\Microsoft.Maui.Controls.resources => 0x8ece1c24 => 13
	i32 2427813419, ; 195: hi\Microsoft.Maui.Controls.resources => 0x90b57e2b => 10
	i32 2435356389, ; 196: System.Console.dll => 0x912896e5 => 113
	i32 2458678730, ; 197: System.Net.Sockets.dll => 0x928c75ca => 133
	i32 2475788418, ; 198: Java.Interop.dll => 0x93918882 => 166
	i32 2480646305, ; 199: Microsoft.Maui.Controls => 0x93dba8a1 => 43
	i32 2538310050, ; 200: System.Reflection.Emit.Lightweight.dll => 0x974b89a2 => 143
	i32 2550873716, ; 201: hr\Microsoft.Maui.Controls.resources => 0x980b3e74 => 11
	i32 2562349572, ; 202: Microsoft.CSharp => 0x98ba5a04 => 105
	i32 2570120770, ; 203: System.Text.Encodings.Web => 0x9930ee42 => 153
	i32 2585220780, ; 204: System.Text.Encoding.Extensions.dll => 0x9a1756ac => 152
	i32 2589602615, ; 205: System.Threading.ThreadPool => 0x9a5a3337 => 158
	i32 2591433303, ; 206: Xamarin.Grpc.Core.dll => 0x9a762257 => 95
	i32 2593496499, ; 207: pl\Microsoft.Maui.Controls.resources => 0x9a959db3 => 20
	i32 2605712449, ; 208: Xamarin.KotlinX.Coroutines.Core.Jvm => 0x9b500441 => 102
	i32 2617129537, ; 209: System.Private.Xml.dll => 0x9bfe3a41 => 141
	i32 2620871830, ; 210: Xamarin.AndroidX.CursorAdapter.dll => 0x9c375496 => 64
	i32 2626831493, ; 211: ja\Microsoft.Maui.Controls.resources => 0x9c924485 => 15
	i32 2640452924, ; 212: Xamarin.Grpc.Protobuf.Lite => 0x9d621d3c => 97
	i32 2663698177, ; 213: System.Runtime.Loader => 0x9ec4cf01 => 146
	i32 2664396074, ; 214: System.Xml.XDocument.dll => 0x9ecf752a => 162
	i32 2665622720, ; 215: System.Drawing.Primitives => 0x9ee22cc0 => 118
	i32 2676780864, ; 216: System.Data.Common.dll => 0x9f8c6f40 => 115
	i32 2715831284, ; 217: Xamarin.Firebase.ProtoliteWellKnownTypes.dll => 0xa1e04bf4 => 88
	i32 2724373263, ; 218: System.Runtime.Numerics.dll => 0xa262a30f => 147
	i32 2732626843, ; 219: Xamarin.AndroidX.Activity => 0xa2e0939b => 56
	i32 2735172069, ; 220: System.Threading.Channels => 0xa30769e5 => 156
	i32 2737747696, ; 221: Xamarin.AndroidX.AppCompat.AppCompatResources.dll => 0xa32eb6f0 => 58
	i32 2752363754, ; 222: Xamarin.Firebase.Firestore.dll => 0xa40dbcea => 87
	i32 2752995522, ; 223: pt-BR\Microsoft.Maui.Controls.resources => 0xa41760c2 => 21
	i32 2758225723, ; 224: Microsoft.Maui.Controls.Xaml => 0xa4672f3b => 44
	i32 2764765095, ; 225: Microsoft.Maui.dll => 0xa4caf7a7 => 45
	i32 2778768386, ; 226: Xamarin.AndroidX.ViewPager.dll => 0xa5a0a402 => 80
	i32 2785988530, ; 227: th\Microsoft.Maui.Controls.resources => 0xa60ecfb2 => 27
	i32 2801831435, ; 228: Microsoft.Maui.Graphics => 0xa7008e0b => 47
	i32 2804607052, ; 229: Xamarin.Firebase.Components.dll => 0xa72ae84c => 85
	i32 2806116107, ; 230: es/Microsoft.Maui.Controls.resources.dll => 0xa741ef0b => 6
	i32 2806986428, ; 231: Plugin.CurrentActivity.dll => 0xa74f36bc => 51
	i32 2810250172, ; 232: Xamarin.AndroidX.CoordinatorLayout.dll => 0xa78103bc => 62
	i32 2831556043, ; 233: nl/Microsoft.Maui.Controls.resources.dll => 0xa8c61dcb => 19
	i32 2853208004, ; 234: Xamarin.AndroidX.ViewPager => 0xaa107fc4 => 80
	i32 2856624150, ; 235: Xamarin.Grpc.Core => 0xaa44a016 => 95
	i32 2861189240, ; 236: Microsoft.Maui.Essentials => 0xaa8a4878 => 46
	i32 2885620179, ; 237: Plugin.CloudFirestore => 0xabff11d3 => 50
	i32 2905242038, ; 238: mscorlib.dll => 0xad2a79b6 => 164
	i32 2909740682, ; 239: System.Private.CoreLib => 0xad6f1e8a => 165
	i32 2916838712, ; 240: Xamarin.AndroidX.ViewPager2.dll => 0xaddb6d38 => 81
	i32 2919462931, ; 241: System.Numerics.Vectors.dll => 0xae037813 => 137
	i32 2943219317, ; 242: Square.OkIO => 0xaf6df675 => 54
	i32 2959614098, ; 243: System.ComponentModel.dll => 0xb0682092 => 112
	i32 2978675010, ; 244: Xamarin.AndroidX.DrawerLayout => 0xb18af942 => 66
	i32 3038032645, ; 245: _Microsoft.Android.Resource.Designer.dll => 0xb514b305 => 34
	i32 3057625584, ; 246: Xamarin.AndroidX.Navigation.Common => 0xb63fa9f0 => 73
	i32 3058099980, ; 247: Xamarin.GooglePlayServices.Tasks => 0xb646e70c => 91
	i32 3059408633, ; 248: Mono.Android.Runtime => 0xb65adef9 => 167
	i32 3059793426, ; 249: System.ComponentModel.Primitives => 0xb660be12 => 110
	i32 3071899978, ; 250: Xamarin.Firebase.Common.dll => 0xb719794a => 84
	i32 3077302341, ; 251: hu/Microsoft.Maui.Controls.resources.dll => 0xb76be845 => 12
	i32 3103600923, ; 252: System.Formats.Asn1 => 0xb8fd311b => 120
	i32 3159123045, ; 253: System.Reflection.Primitives.dll => 0xbc4c6465 => 144
	i32 3178803400, ; 254: Xamarin.AndroidX.Navigation.Fragment.dll => 0xbd78b0c8 => 74
	i32 3220365878, ; 255: System.Threading => 0xbff2e236 => 159
	i32 3230466174, ; 256: Xamarin.GooglePlayServices.Basement.dll => 0xc08d007e => 90
	i32 3258312781, ; 257: Xamarin.AndroidX.CardView => 0xc235e84d => 59
	i32 3265493905, ; 258: System.Linq.Queryable.dll => 0xc2a37b91 => 124
	i32 3305363605, ; 259: fi\Microsoft.Maui.Controls.resources => 0xc503d895 => 7
	i32 3311114310, ; 260: ChibitsLink => 0xc55b9846 => 104
	i32 3316684772, ; 261: System.Net.Requests.dll => 0xc5b097e4 => 131
	i32 3317135071, ; 262: Xamarin.AndroidX.CustomView.dll => 0xc5b776df => 65
	i32 3346324047, ; 263: Xamarin.AndroidX.Navigation.Runtime => 0xc774da4f => 75
	i32 3357674450, ; 264: ru\Microsoft.Maui.Controls.resources => 0xc8220bd2 => 24
	i32 3358260929, ; 265: System.Text.Json => 0xc82afec1 => 154
	i32 3362522851, ; 266: Xamarin.AndroidX.Core => 0xc86c06e3 => 63
	i32 3366347497, ; 267: Java.Interop => 0xc8a662e9 => 166
	i32 3374999561, ; 268: Xamarin.AndroidX.RecyclerView => 0xc92a6809 => 77
	i32 3381016424, ; 269: da\Microsoft.Maui.Controls.resources => 0xc9863768 => 3
	i32 3428513518, ; 270: Microsoft.Extensions.DependencyInjection.dll => 0xcc5af6ee => 37
	i32 3463511458, ; 271: hr/Microsoft.Maui.Controls.resources.dll => 0xce70fda2 => 11
	i32 3471940407, ; 272: System.ComponentModel.TypeConverter.dll => 0xcef19b37 => 111
	i32 3473879593, ; 273: Xamarin.Grpc.OkHttp.dll => 0xcf0f3229 => 96
	i32 3476120550, ; 274: Mono.Android => 0xcf3163e6 => 168
	i32 3479583265, ; 275: ru/Microsoft.Maui.Controls.resources.dll => 0xcf663a21 => 24
	i32 3484440000, ; 276: ro\Microsoft.Maui.Controls.resources => 0xcfb055c0 => 23
	i32 3485117614, ; 277: System.Text.Json.dll => 0xcfbaacae => 154
	i32 3509114376, ; 278: System.Xml.Linq => 0xd128d608 => 160
	i32 3526024462, ; 279: ChibitsLink.dll => 0xd22add0e => 104
	i32 3580758918, ; 280: zh-HK\Microsoft.Maui.Controls.resources => 0xd56e0b86 => 31
	i32 3597794883, ; 281: Xamarin.Firebase.Firestore => 0xd671fe43 => 87
	i32 3598340787, ; 282: System.Net.WebSockets.Client => 0xd67a52b3 => 135
	i32 3608519521, ; 283: System.Linq.dll => 0xd715a361 => 125
	i32 3641597786, ; 284: Xamarin.AndroidX.Lifecycle.LiveData.Core => 0xd90e5f5a => 69
	i32 3643446276, ; 285: tr\Microsoft.Maui.Controls.resources => 0xd92a9404 => 28
	i32 3643854240, ; 286: Xamarin.AndroidX.Navigation.Fragment => 0xd930cda0 => 74
	i32 3657292374, ; 287: Microsoft.Extensions.Configuration.Abstractions.dll => 0xd9fdda56 => 36
	i32 3660523487, ; 288: System.Net.NetworkInformation => 0xda2f27df => 129
	i32 3672681054, ; 289: Mono.Android.dll => 0xdae8aa5e => 168
	i32 3697841164, ; 290: zh-Hant/Microsoft.Maui.Controls.resources.dll => 0xdc68940c => 33
	i32 3724971120, ; 291: Xamarin.AndroidX.Navigation.Common.dll => 0xde068c70 => 73
	i32 3731644420, ; 292: System.Reactive => 0xde6c6004 => 55
	i32 3732100267, ; 293: System.Net.NameResolution => 0xde7354ab => 128
	i32 3748608112, ; 294: System.Diagnostics.DiagnosticSource => 0xdf6f3870 => 116
	i32 3771698872, ; 295: Xamarin.Io.OpenCensus.OpenCensusApi => 0xe0cf8eb8 => 99
	i32 3786282454, ; 296: Xamarin.AndroidX.Collection => 0xe1ae15d6 => 60
	i32 3792276235, ; 297: System.Collections.NonGeneric => 0xe2098b0b => 107
	i32 3802395368, ; 298: System.Collections.Specialized.dll => 0xe2a3f2e8 => 108
	i32 3823082795, ; 299: System.Security.Cryptography.dll => 0xe3df9d2b => 151
	i32 3841636137, ; 300: Microsoft.Extensions.DependencyInjection.Abstractions.dll => 0xe4fab729 => 38
	i32 3849253459, ; 301: System.Runtime.InteropServices.dll => 0xe56ef253 => 145
	i32 3885497537, ; 302: System.Net.WebHeaderCollection.dll => 0xe797fcc1 => 134
	i32 3889960447, ; 303: zh-Hans/Microsoft.Maui.Controls.resources.dll => 0xe7dc15ff => 32
	i32 3896106733, ; 304: System.Collections.Concurrent.dll => 0xe839deed => 106
	i32 3896760992, ; 305: Xamarin.AndroidX.Core.dll => 0xe843daa0 => 63
	i32 3910130544, ; 306: Xamarin.AndroidX.Collection.Jvm => 0xe90fdb70 => 61
	i32 3928044579, ; 307: System.Xml.ReaderWriter => 0xea213423 => 161
	i32 3931092270, ; 308: Xamarin.AndroidX.Navigation.UI => 0xea4fb52e => 76
	i32 3943739589, ; 309: Xamarin.Grpc.Context.dll => 0xeb10b0c5 => 94
	i32 3955647286, ; 310: Xamarin.AndroidX.AppCompat.dll => 0xebc66336 => 57
	i32 3968844647, ; 311: Xamarin.Protobuf.Lite.dll => 0xec8fc367 => 103
	i32 3970018735, ; 312: Xamarin.GooglePlayServices.Tasks.dll => 0xeca1adaf => 91
	i32 3980434154, ; 313: th/Microsoft.Maui.Controls.resources.dll => 0xed409aea => 27
	i32 3987592930, ; 314: he/Microsoft.Maui.Controls.resources.dll => 0xedadd6e2 => 9
	i32 4025784931, ; 315: System.Memory => 0xeff49a63 => 126
	i32 4046471985, ; 316: Microsoft.Maui.Controls.Xaml.dll => 0xf1304331 => 44
	i32 4054681211, ; 317: System.Reflection.Emit.ILGeneration => 0xf1ad867b => 142
	i32 4068434129, ; 318: System.Private.Xml.Linq.dll => 0xf27f60d1 => 140
	i32 4073602200, ; 319: System.Threading.dll => 0xf2ce3c98 => 159
	i32 4094352644, ; 320: Microsoft.Maui.Essentials.dll => 0xf40add04 => 46
	i32 4099507663, ; 321: System.Drawing.dll => 0xf45985cf => 119
	i32 4100113165, ; 322: System.Private.Uri => 0xf462c30d => 139
	i32 4102112229, ; 323: pt/Microsoft.Maui.Controls.resources.dll => 0xf48143e5 => 22
	i32 4125707920, ; 324: ms/Microsoft.Maui.Controls.resources.dll => 0xf5e94e90 => 17
	i32 4126470640, ; 325: Microsoft.Extensions.DependencyInjection => 0xf5f4f1f0 => 37
	i32 4147896353, ; 326: System.Reflection.Emit.ILGeneration.dll => 0xf73be021 => 142
	i32 4150914736, ; 327: uk\Microsoft.Maui.Controls.resources => 0xf769eeb0 => 29
	i32 4151237749, ; 328: System.Core => 0xf76edc75 => 114
	i32 4181436372, ; 329: System.Runtime.Serialization.Primitives => 0xf93ba7d4 => 149
	i32 4182413190, ; 330: Xamarin.AndroidX.Lifecycle.ViewModelSavedState.dll => 0xf94a8f86 => 71
	i32 4200179444, ; 331: Plugin.FirebaseAuth => 0xfa59a6f4 => 52
	i32 4213026141, ; 332: System.Diagnostics.DiagnosticSource.dll => 0xfb1dad5d => 116
	i32 4223148364, ; 333: Xamarin.Grpc.Android.dll => 0xfbb8214c => 92
	i32 4271975918, ; 334: Microsoft.Maui.Controls.dll => 0xfea12dee => 43
	i32 4274976490, ; 335: System.Runtime.Numerics => 0xfecef6ea => 147
	i32 4284549794, ; 336: Xamarin.Firebase.Components => 0xff610aa2 => 85
	i32 4292120959 ; 337: Xamarin.AndroidX.Lifecycle.ViewModelSavedState => 0xffd4917f => 71
], align 4

@assembly_image_cache_indices = dso_local local_unnamed_addr constant [338 x i32] [
	i32 129, ; 0
	i32 97, ; 1
	i32 92, ; 2
	i32 128, ; 3
	i32 48, ; 4
	i32 136, ; 5
	i32 157, ; 6
	i32 33, ; 7
	i32 47, ; 8
	i32 145, ; 9
	i32 156, ; 10
	i32 134, ; 11
	i32 60, ; 12
	i32 79, ; 13
	i32 30, ; 14
	i32 31, ; 15
	i32 112, ; 16
	i32 105, ; 17
	i32 143, ; 18
	i32 96, ; 19
	i32 49, ; 20
	i32 2, ; 21
	i32 30, ; 22
	i32 56, ; 23
	i32 15, ; 24
	i32 68, ; 25
	i32 14, ; 26
	i32 157, ; 27
	i32 126, ; 28
	i32 158, ; 29
	i32 34, ; 30
	i32 52, ; 31
	i32 26, ; 32
	i32 109, ; 33
	i32 67, ; 34
	i32 86, ; 35
	i32 149, ; 36
	i32 164, ; 37
	i32 163, ; 38
	i32 138, ; 39
	i32 13, ; 40
	i32 7, ; 41
	i32 42, ; 42
	i32 124, ; 43
	i32 39, ; 44
	i32 21, ; 45
	i32 55, ; 46
	i32 65, ; 47
	i32 19, ; 48
	i32 153, ; 49
	i32 106, ; 50
	i32 132, ; 51
	i32 1, ; 52
	i32 160, ; 53
	i32 16, ; 54
	i32 4, ; 55
	i32 93, ; 56
	i32 146, ; 57
	i32 131, ; 58
	i32 122, ; 59
	i32 25, ; 60
	i32 41, ; 61
	i32 115, ; 62
	i32 139, ; 63
	i32 121, ; 64
	i32 49, ; 65
	i32 50, ; 66
	i32 110, ; 67
	i32 28, ; 68
	i32 48, ; 69
	i32 51, ; 70
	i32 54, ; 71
	i32 68, ; 72
	i32 162, ; 73
	i32 109, ; 74
	i32 78, ; 75
	i32 119, ; 76
	i32 38, ; 77
	i32 3, ; 78
	i32 57, ; 79
	i32 118, ; 80
	i32 123, ; 81
	i32 99, ; 82
	i32 70, ; 83
	i32 111, ; 84
	i32 101, ; 85
	i32 163, ; 86
	i32 82, ; 87
	i32 16, ; 88
	i32 98, ; 89
	i32 22, ; 90
	i32 75, ; 91
	i32 20, ; 92
	i32 53, ; 93
	i32 98, ; 94
	i32 18, ; 95
	i32 103, ; 96
	i32 61, ; 97
	i32 2, ; 98
	i32 100, ; 99
	i32 66, ; 100
	i32 125, ; 101
	i32 84, ; 102
	i32 32, ; 103
	i32 78, ; 104
	i32 62, ; 105
	i32 148, ; 106
	i32 83, ; 107
	i32 0, ; 108
	i32 120, ; 109
	i32 132, ; 110
	i32 6, ; 111
	i32 122, ; 112
	i32 58, ; 113
	i32 42, ; 114
	i32 121, ; 115
	i32 10, ; 116
	i32 5, ; 117
	i32 155, ; 118
	i32 93, ; 119
	i32 25, ; 120
	i32 135, ; 121
	i32 72, ; 122
	i32 81, ; 123
	i32 64, ; 124
	i32 127, ; 125
	i32 155, ; 126
	i32 150, ; 127
	i32 89, ; 128
	i32 86, ; 129
	i32 130, ; 130
	i32 136, ; 131
	i32 151, ; 132
	i32 59, ; 133
	i32 23, ; 134
	i32 1, ; 135
	i32 117, ; 136
	i32 79, ; 137
	i32 39, ; 138
	i32 114, ; 139
	i32 167, ; 140
	i32 17, ; 141
	i32 100, ; 142
	i32 67, ; 143
	i32 9, ; 144
	i32 72, ; 145
	i32 101, ; 146
	i32 89, ; 147
	i32 76, ; 148
	i32 152, ; 149
	i32 148, ; 150
	i32 40, ; 151
	i32 29, ; 152
	i32 26, ; 153
	i32 123, ; 154
	i32 144, ; 155
	i32 83, ; 156
	i32 8, ; 157
	i32 90, ; 158
	i32 107, ; 159
	i32 140, ; 160
	i32 35, ; 161
	i32 5, ; 162
	i32 70, ; 163
	i32 0, ; 164
	i32 141, ; 165
	i32 69, ; 166
	i32 4, ; 167
	i32 117, ; 168
	i32 150, ; 169
	i32 88, ; 170
	i32 137, ; 171
	i32 113, ; 172
	i32 108, ; 173
	i32 45, ; 174
	i32 12, ; 175
	i32 41, ; 176
	i32 40, ; 177
	i32 138, ; 178
	i32 94, ; 179
	i32 102, ; 180
	i32 127, ; 181
	i32 14, ; 182
	i32 36, ; 183
	i32 8, ; 184
	i32 77, ; 185
	i32 133, ; 186
	i32 18, ; 187
	i32 165, ; 188
	i32 53, ; 189
	i32 130, ; 190
	i32 161, ; 191
	i32 35, ; 192
	i32 82, ; 193
	i32 13, ; 194
	i32 10, ; 195
	i32 113, ; 196
	i32 133, ; 197
	i32 166, ; 198
	i32 43, ; 199
	i32 143, ; 200
	i32 11, ; 201
	i32 105, ; 202
	i32 153, ; 203
	i32 152, ; 204
	i32 158, ; 205
	i32 95, ; 206
	i32 20, ; 207
	i32 102, ; 208
	i32 141, ; 209
	i32 64, ; 210
	i32 15, ; 211
	i32 97, ; 212
	i32 146, ; 213
	i32 162, ; 214
	i32 118, ; 215
	i32 115, ; 216
	i32 88, ; 217
	i32 147, ; 218
	i32 56, ; 219
	i32 156, ; 220
	i32 58, ; 221
	i32 87, ; 222
	i32 21, ; 223
	i32 44, ; 224
	i32 45, ; 225
	i32 80, ; 226
	i32 27, ; 227
	i32 47, ; 228
	i32 85, ; 229
	i32 6, ; 230
	i32 51, ; 231
	i32 62, ; 232
	i32 19, ; 233
	i32 80, ; 234
	i32 95, ; 235
	i32 46, ; 236
	i32 50, ; 237
	i32 164, ; 238
	i32 165, ; 239
	i32 81, ; 240
	i32 137, ; 241
	i32 54, ; 242
	i32 112, ; 243
	i32 66, ; 244
	i32 34, ; 245
	i32 73, ; 246
	i32 91, ; 247
	i32 167, ; 248
	i32 110, ; 249
	i32 84, ; 250
	i32 12, ; 251
	i32 120, ; 252
	i32 144, ; 253
	i32 74, ; 254
	i32 159, ; 255
	i32 90, ; 256
	i32 59, ; 257
	i32 124, ; 258
	i32 7, ; 259
	i32 104, ; 260
	i32 131, ; 261
	i32 65, ; 262
	i32 75, ; 263
	i32 24, ; 264
	i32 154, ; 265
	i32 63, ; 266
	i32 166, ; 267
	i32 77, ; 268
	i32 3, ; 269
	i32 37, ; 270
	i32 11, ; 271
	i32 111, ; 272
	i32 96, ; 273
	i32 168, ; 274
	i32 24, ; 275
	i32 23, ; 276
	i32 154, ; 277
	i32 160, ; 278
	i32 104, ; 279
	i32 31, ; 280
	i32 87, ; 281
	i32 135, ; 282
	i32 125, ; 283
	i32 69, ; 284
	i32 28, ; 285
	i32 74, ; 286
	i32 36, ; 287
	i32 129, ; 288
	i32 168, ; 289
	i32 33, ; 290
	i32 73, ; 291
	i32 55, ; 292
	i32 128, ; 293
	i32 116, ; 294
	i32 99, ; 295
	i32 60, ; 296
	i32 107, ; 297
	i32 108, ; 298
	i32 151, ; 299
	i32 38, ; 300
	i32 145, ; 301
	i32 134, ; 302
	i32 32, ; 303
	i32 106, ; 304
	i32 63, ; 305
	i32 61, ; 306
	i32 161, ; 307
	i32 76, ; 308
	i32 94, ; 309
	i32 57, ; 310
	i32 103, ; 311
	i32 91, ; 312
	i32 27, ; 313
	i32 9, ; 314
	i32 126, ; 315
	i32 44, ; 316
	i32 142, ; 317
	i32 140, ; 318
	i32 159, ; 319
	i32 46, ; 320
	i32 119, ; 321
	i32 139, ; 322
	i32 22, ; 323
	i32 17, ; 324
	i32 37, ; 325
	i32 142, ; 326
	i32 29, ; 327
	i32 114, ; 328
	i32 149, ; 329
	i32 71, ; 330
	i32 52, ; 331
	i32 116, ; 332
	i32 92, ; 333
	i32 43, ; 334
	i32 147, ; 335
	i32 85, ; 336
	i32 71 ; 337
], align 4

@marshal_methods_number_of_classes = dso_local local_unnamed_addr constant i32 0, align 4

@marshal_methods_class_cache = dso_local local_unnamed_addr global [0 x %struct.MarshalMethodsManagedClass] zeroinitializer, align 4

; Names of classes in which marshal methods reside
@mm_class_names = dso_local local_unnamed_addr constant [0 x ptr] zeroinitializer, align 4

@mm_method_names = dso_local local_unnamed_addr constant [1 x %struct.MarshalMethodName] [
	%struct.MarshalMethodName {
		i64 0, ; id 0x0; name: 
		ptr @.MarshalMethodName.0_name; char* name
	} ; 0
], align 8

; get_function_pointer (uint32_t mono_image_index, uint32_t class_index, uint32_t method_token, void*& target_ptr)
@get_function_pointer = internal dso_local unnamed_addr global ptr null, align 4

; Functions

; Function attributes: "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" nofree norecurse nosync nounwind "stack-protector-buffer-size"="8" uwtable willreturn
define void @xamarin_app_init(ptr nocapture noundef readnone %env, ptr noundef %fn) local_unnamed_addr #0
{
	%fnIsNull = icmp eq ptr %fn, null
	br i1 %fnIsNull, label %1, label %2

1: ; preds = %0
	%putsResult = call noundef i32 @puts(ptr @.str.0)
	call void @abort()
	unreachable 

2: ; preds = %1, %0
	store ptr %fn, ptr @get_function_pointer, align 4, !tbaa !3
	ret void
}

; Strings
@.str.0 = private unnamed_addr constant [40 x i8] c"get_function_pointer MUST be specified\0A\00", align 1

;MarshalMethodName
@.MarshalMethodName.0_name = private unnamed_addr constant [1 x i8] c"\00", align 1

; External functions

; Function attributes: "no-trapping-math"="true" noreturn nounwind "stack-protector-buffer-size"="8"
declare void @abort() local_unnamed_addr #2

; Function attributes: nofree nounwind
declare noundef i32 @puts(ptr noundef) local_unnamed_addr #1
attributes #0 = { "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" nofree norecurse nosync nounwind "stack-protector-buffer-size"="8" "stackrealign" "target-cpu"="i686" "target-features"="+cx8,+mmx,+sse,+sse2,+sse3,+ssse3,+x87" "tune-cpu"="generic" uwtable willreturn }
attributes #1 = { nofree nounwind }
attributes #2 = { "no-trapping-math"="true" noreturn nounwind "stack-protector-buffer-size"="8" "stackrealign" "target-cpu"="i686" "target-features"="+cx8,+mmx,+sse,+sse2,+sse3,+ssse3,+x87" "tune-cpu"="generic" }

; Metadata
!llvm.module.flags = !{!0, !1, !7}
!0 = !{i32 1, !"wchar_size", i32 4}
!1 = !{i32 7, !"PIC Level", i32 2}
!llvm.ident = !{!2}
!2 = !{!"Xamarin.Android remotes/origin/release/8.0.4xx @ 82d8938cf80f6d5fa6c28529ddfbdb753d805ab4"}
!3 = !{!4, !4, i64 0}
!4 = !{!"any pointer", !5, i64 0}
!5 = !{!"omnipotent char", !6, i64 0}
!6 = !{!"Simple C++ TBAA"}
!7 = !{i32 1, !"NumRegisterParameters", i32 0}
