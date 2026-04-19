; ModuleID = 'marshal_methods.arm64-v8a.ll'
source_filename = "marshal_methods.arm64-v8a.ll"
target datalayout = "e-m:e-i8:8:32-i16:16:32-i64:64-i128:128-n32:64-S128"
target triple = "aarch64-unknown-linux-android21"

%struct.MarshalMethodName = type {
	i64, ; uint64_t id
	ptr ; char* name
}

%struct.MarshalMethodsManagedClass = type {
	i32, ; uint32_t token
	ptr ; MonoClass klass
}

@assembly_image_cache = dso_local local_unnamed_addr global [170 x ptr] zeroinitializer, align 8

; Each entry maps hash of an assembly name to an index into the `assembly_image_cache` array
@assembly_image_cache_hashes = dso_local local_unnamed_addr constant [340 x i64] [
	i64 98382396393917666, ; 0: Microsoft.Extensions.Primitives.dll => 0x15d8644ad360ce2 => 42
	i64 120698629574877762, ; 1: Mono.Android => 0x1accec39cafe242 => 169
	i64 131669012237370309, ; 2: Microsoft.Maui.Essentials.dll => 0x1d3c844de55c3c5 => 46
	i64 196720943101637631, ; 3: System.Linq.Expressions.dll => 0x2bae4a7cd73f3ff => 124
	i64 210515253464952879, ; 4: Xamarin.AndroidX.Collection.dll => 0x2ebe681f694702f => 60
	i64 232391251801502327, ; 5: Xamarin.AndroidX.SavedState.dll => 0x3399e9cbc897277 => 78
	i64 233177144301842968, ; 6: Xamarin.AndroidX.Collection.Jvm.dll => 0x33c696097d9f218 => 61
	i64 435118502366263740, ; 7: Xamarin.AndroidX.Security.SecurityCrypto.dll => 0x609d9f8f8bdb9bc => 79
	i64 464346026994987652, ; 8: System.Reactive.dll => 0x671b04057e67284 => 55
	i64 515298607468333944, ; 9: Xamarin.Io.OpenCensus.OpenCensusContribGrpcMetrics => 0x726b55a73674b78 => 101
	i64 545109961164950392, ; 10: fi/Microsoft.Maui.Controls.resources.dll => 0x7909e9f1ec38b78 => 7
	i64 560278790331054453, ; 11: System.Reflection.Primitives => 0x7c6829760de3975 => 145
	i64 733635950719068722, ; 12: Plugin.FirebaseAuth => 0xa2e66029d10ea32 => 52
	i64 750875890346172408, ; 13: System.Threading.Thread => 0xa6ba5a4da7d1ff8 => 158
	i64 799765834175365804, ; 14: System.ComponentModel.dll => 0xb1956c9f18442ac => 113
	i64 849051935479314978, ; 15: hi/Microsoft.Maui.Controls.resources.dll => 0xbc8703ca21a3a22 => 10
	i64 872800313462103108, ; 16: Xamarin.AndroidX.DrawerLayout => 0xc1ccf42c3c21c44 => 66
	i64 1010599046655515943, ; 17: System.Reflection.Primitives.dll => 0xe065e7a82401d27 => 145
	i64 1120440138749646132, ; 18: Xamarin.Google.Android.Material.dll => 0xf8c9a5eae431534 => 90
	i64 1121665720830085036, ; 19: nb/Microsoft.Maui.Controls.resources.dll => 0xf90f507becf47ac => 18
	i64 1268860745194512059, ; 20: System.Drawing.dll => 0x119be62002c19ebb => 120
	i64 1274338068859211160, ; 21: Xamarin.Grpc.Api => 0x11af5bb8ce1c4d98 => 94
	i64 1305330500145730299, ; 22: Xamarin.Io.OpenCensus.OpenCensusApi.dll => 0x121d772c87ab52fb => 100
	i64 1368633735297491523, ; 23: Xamarin.Firebase.Database.Collection.dll => 0x12fe5d218405e243 => 87
	i64 1369545283391376210, ; 24: Xamarin.AndroidX.Navigation.Fragment.dll => 0x13019a2dd85acb52 => 74
	i64 1392315331768750440, ; 25: Xamarin.Firebase.Auth.Interop.dll => 0x13527f6add681168 => 84
	i64 1465843056802068477, ; 26: Xamarin.Firebase.Components.dll => 0x1457b87e6928f7fd => 86
	i64 1474586420366808421, ; 27: Xamarin.Grpc.Android.dll => 0x1476c88960941565 => 93
	i64 1476839205573959279, ; 28: System.Net.Primitives.dll => 0x147ec96ece9b1e6f => 131
	i64 1486715745332614827, ; 29: Microsoft.Maui.Controls.dll => 0x14a1e017ea87d6ab => 43
	i64 1513467482682125403, ; 30: Mono.Android.Runtime => 0x1500eaa8245f6c5b => 168
	i64 1537168428375924959, ; 31: System.Threading.Thread.dll => 0x15551e8a954ae0df => 158
	i64 1556147632182429976, ; 32: ko/Microsoft.Maui.Controls.resources.dll => 0x15988c06d24c8918 => 16
	i64 1624659445732251991, ; 33: Xamarin.AndroidX.AppCompat.AppCompatResources.dll => 0x168bf32877da9957 => 58
	i64 1628611045998245443, ; 34: Xamarin.AndroidX.Lifecycle.ViewModelSavedState.dll => 0x1699fd1e1a00b643 => 71
	i64 1712733211100389100, ; 35: ChibitsLink => 0x17c4d9c7f2a71eec => 105
	i64 1731380447121279447, ; 36: Newtonsoft.Json => 0x18071957e9b889d7 => 48
	i64 1743969030606105336, ; 37: System.Memory.dll => 0x1833d297e88f2af8 => 127
	i64 1767386781656293639, ; 38: System.Private.Uri.dll => 0x188704e9f5582107 => 140
	i64 1795316252682057001, ; 39: Xamarin.AndroidX.AppCompat.dll => 0x18ea3e9eac997529 => 57
	i64 1835311033149317475, ; 40: es\Microsoft.Maui.Controls.resources => 0x197855a927386163 => 6
	i64 1836611346387731153, ; 41: Xamarin.AndroidX.SavedState => 0x197cf449ebe482d1 => 78
	i64 1875417405349196092, ; 42: System.Drawing.Primitives => 0x1a06d2319b6c713c => 119
	i64 1881198190668717030, ; 43: tr\Microsoft.Maui.Controls.resources => 0x1a1b5bc992ea9be6 => 28
	i64 1897575647115118287, ; 44: Xamarin.AndroidX.Security.SecurityCrypto => 0x1a558aff4cba86cf => 79
	i64 1920760634179481754, ; 45: Microsoft.Maui.Controls.Xaml => 0x1aa7e99ec2d2709a => 44
	i64 1956817255800234857, ; 46: Xamarin.Grpc.Android => 0x1b2802ed2e53e369 => 93
	i64 1959996714666907089, ; 47: tr/Microsoft.Maui.Controls.resources.dll => 0x1b334ea0a2a755d1 => 28
	i64 1981742497975770890, ; 48: Xamarin.AndroidX.Lifecycle.ViewModel.dll => 0x1b80904d5c241f0a => 70
	i64 1983698669889758782, ; 49: cs/Microsoft.Maui.Controls.resources.dll => 0x1b87836e2031a63e => 2
	i64 1990714127648872464, ; 50: Xamarin.Grpc.Core.dll => 0x1ba06ff3abdcd810 => 96
	i64 2019660174692588140, ; 51: pl/Microsoft.Maui.Controls.resources.dll => 0x1c07463a6f8e1a6c => 20
	i64 2102659300918482391, ; 52: System.Drawing.Primitives.dll => 0x1d2e257e6aead5d7 => 119
	i64 2133195048986300728, ; 53: Newtonsoft.Json.dll => 0x1d9aa1984b735138 => 48
	i64 2262844636196693701, ; 54: Xamarin.AndroidX.DrawerLayout.dll => 0x1f673d352266e6c5 => 66
	i64 2287834202362508563, ; 55: System.Collections.Concurrent => 0x1fc00515e8ce7513 => 107
	i64 2302323944321350744, ; 56: ru/Microsoft.Maui.Controls.resources.dll => 0x1ff37f6ddb267c58 => 24
	i64 2329709569556905518, ; 57: Xamarin.AndroidX.Lifecycle.LiveData.Core.dll => 0x2054ca829b447e2e => 69
	i64 2335503487726329082, ; 58: System.Text.Encodings.Web => 0x2069600c4d9d1cfa => 154
	i64 2343783402604882194, ; 59: Xamarin.Grpc.Stub.dll => 0x2086ca9636b86912 => 99
	i64 2470498323731680442, ; 60: Xamarin.AndroidX.CoordinatorLayout => 0x2248f922dc398cba => 62
	i64 2497223385847772520, ; 61: System.Runtime => 0x22a7eb7046413568 => 151
	i64 2547086958574651984, ; 62: Xamarin.AndroidX.Activity.dll => 0x2359121801df4a50 => 56
	i64 2602673633151553063, ; 63: th\Microsoft.Maui.Controls.resources => 0x241e8de13a460e27 => 27
	i64 2624866290265602282, ; 64: mscorlib.dll => 0x246d65fbde2db8ea => 165
	i64 2632269733008246987, ; 65: System.Net.NameResolution => 0x2487b36034f808cb => 129
	i64 2656907746661064104, ; 66: Microsoft.Extensions.DependencyInjection => 0x24df3b84c8b75da8 => 37
	i64 2662981627730767622, ; 67: cs\Microsoft.Maui.Controls.resources => 0x24f4cfae6c48af06 => 2
	i64 2706075432581334785, ; 68: System.Net.WebSockets => 0x258de944be6c0701 => 137
	i64 2801558180824670388, ; 69: Plugin.CurrentActivity.dll => 0x26e1225279a4e0b4 => 51
	i64 2895129759130297543, ; 70: fi\Microsoft.Maui.Controls.resources => 0x282d912d479fa4c7 => 7
	i64 2951672403965468947, ; 71: Xamarin.Firebase.Database.Collection => 0x28f67269abaf6113 => 87
	i64 3017704767998173186, ; 72: Xamarin.Google.Android.Material => 0x29e10a7f7d88a002 => 90
	i64 3171992396844006720, ; 73: Square.OkIO => 0x2c052e476c207d40 => 54
	i64 3289520064315143713, ; 74: Xamarin.AndroidX.Lifecycle.Common => 0x2da6b911e3063621 => 68
	i64 3311221304742556517, ; 75: System.Numerics.Vectors.dll => 0x2df3d23ba9e2b365 => 138
	i64 3325875462027654285, ; 76: System.Runtime.Numerics => 0x2e27e21c8958b48d => 148
	i64 3328853167529574890, ; 77: System.Net.Sockets.dll => 0x2e327651a008c1ea => 134
	i64 3344514922410554693, ; 78: Xamarin.KotlinX.Coroutines.Core.Jvm => 0x2e6a1a9a18463545 => 103
	i64 3364695309916733813, ; 79: Xamarin.Firebase.Common => 0x2eb1cc8eb5028175 => 85
	i64 3411255996856937470, ; 80: Xamarin.GooglePlayServices.Basement => 0x2f5737416a942bfe => 91
	i64 3429672777697402584, ; 81: Microsoft.Maui.Essentials => 0x2f98a5385a7b1ed8 => 46
	i64 3494946837667399002, ; 82: Microsoft.Extensions.Configuration => 0x30808ba1c00a455a => 35
	i64 3522470458906976663, ; 83: Xamarin.AndroidX.SwipeRefreshLayout => 0x30e2543832f52197 => 80
	i64 3551103847008531295, ; 84: System.Private.CoreLib.dll => 0x31480e226177735f => 166
	i64 3567343442040498961, ; 85: pt\Microsoft.Maui.Controls.resources => 0x3181bff5bea4ab11 => 22
	i64 3571415421602489686, ; 86: System.Runtime.dll => 0x319037675df7e556 => 151
	i64 3609787854626478660, ; 87: Plugin.CurrentActivity => 0x32188aeda587da44 => 51
	i64 3638003163729360188, ; 88: Microsoft.Extensions.Configuration.Abstractions => 0x327cc89a39d5f53c => 36
	i64 3647754201059316852, ; 89: System.Xml.ReaderWriter => 0x329f6d1e86145474 => 162
	i64 3655542548057982301, ; 90: Microsoft.Extensions.Configuration.dll => 0x32bb18945e52855d => 35
	i64 3727469159507183293, ; 91: Xamarin.AndroidX.RecyclerView => 0x33baa1739ba646bd => 77
	i64 3869221888984012293, ; 92: Microsoft.Extensions.Logging.dll => 0x35b23cceda0ed605 => 39
	i64 3890352374528606784, ; 93: Microsoft.Maui.Controls.Xaml.dll => 0x35fd4edf66e00240 => 44
	i64 3933965368022646939, ; 94: System.Net.Requests => 0x369840a8bfadc09b => 132
	i64 3966267475168208030, ; 95: System.Memory => 0x370b03412596249e => 127
	i64 4009997192427317104, ; 96: System.Runtime.Serialization.Primitives => 0x37a65f335cf1a770 => 150
	i64 4045730230152541805, ; 97: Xamarin.Grpc.Protobuf.Lite.dll => 0x38255235894d366d => 98
	i64 4073500526318903918, ; 98: System.Private.Xml.dll => 0x3887fb25779ae26e => 142
	i64 4120493066591692148, ; 99: zh-Hant\Microsoft.Maui.Controls.resources => 0x392eee9cdda86574 => 33
	i64 4154383907710350974, ; 100: System.ComponentModel => 0x39a7562737acb67e => 113
	i64 4167269041631776580, ; 101: System.Threading.ThreadPool => 0x39d51d1d3df1cf44 => 159
	i64 4187479170553454871, ; 102: System.Linq.Expressions => 0x3a1cea1e912fa117 => 124
	i64 4205801962323029395, ; 103: System.ComponentModel.TypeConverter => 0x3a5e0299f7e7ad93 => 112
	i64 4247996603072512073, ; 104: Xamarin.GooglePlayServices.Tasks => 0x3af3ea6755340049 => 92
	i64 4356591372459378815, ; 105: vi/Microsoft.Maui.Controls.resources.dll => 0x3c75b8c562f9087f => 30
	i64 4679594760078841447, ; 106: ar/Microsoft.Maui.Controls.resources.dll => 0x40f142a407475667 => 0
	i64 4702770163853758138, ; 107: Xamarin.Firebase.Components => 0x4143988c34cf0eba => 86
	i64 4794310189461587505, ; 108: Xamarin.AndroidX.Activity => 0x4288cfb749e4c631 => 56
	i64 4795410492532947900, ; 109: Xamarin.AndroidX.SwipeRefreshLayout.dll => 0x428cb86f8f9b7bbc => 80
	i64 4809057822547766521, ; 110: System.Drawing => 0x42bd349c3145ecf9 => 120
	i64 4814660307502931973, ; 111: System.Net.NameResolution.dll => 0x42d11c0a5ee2a005 => 129
	i64 4853321196694829351, ; 112: System.Runtime.Loader.dll => 0x435a75ea15de7927 => 147
	i64 4977709562956556791, ; 113: Xamarin.Io.OpenCensus.OpenCensusApi => 0x45146079771729f7 => 100
	i64 5103417709280584325, ; 114: System.Collections.Specialized => 0x46d2fb5e161b6285 => 109
	i64 5182934613077526976, ; 115: System.Collections.Specialized.dll => 0x47ed7b91fa9009c0 => 109
	i64 5290215063822704973, ; 116: Xamarin.Grpc.Stub => 0x496a9e926092a14d => 99
	i64 5290786973231294105, ; 117: System.Runtime.Loader => 0x496ca6b869b72699 => 147
	i64 5427880336170504416, ; 118: Plugin.FirebaseAuth.dll => 0x4b53b46858c550e0 => 52
	i64 5471532531798518949, ; 119: sv\Microsoft.Maui.Controls.resources => 0x4beec9d926d82ca5 => 26
	i64 5507995362134886206, ; 120: System.Core.dll => 0x4c705499688c873e => 115
	i64 5522859530602327440, ; 121: uk\Microsoft.Maui.Controls.resources => 0x4ca5237b51eead90 => 29
	i64 5570799893513421663, ; 122: System.IO.Compression.Brotli => 0x4d4f74fcdfa6c35f => 122
	i64 5573260873512690141, ; 123: System.Security.Cryptography.dll => 0x4d58333c6e4ea1dd => 152
	i64 5692067934154308417, ; 124: Xamarin.AndroidX.ViewPager2.dll => 0x4efe49a0d4a8bb41 => 82
	i64 5837276141656118154, ; 125: Plugin.CloudFirestore => 0x51022bb93f46938a => 50
	i64 5979151488806146654, ; 126: System.Formats.Asn1 => 0x52fa3699a489d25e => 121
	i64 6068057819846744445, ; 127: ro/Microsoft.Maui.Controls.resources.dll => 0x5436126fec7f197d => 23
	i64 6135981624229292808, ; 128: Xamarin.Grpc.Api.dll => 0x552762c70482eb08 => 94
	i64 6200764641006662125, ; 129: ro\Microsoft.Maui.Controls.resources => 0x560d8a96830131ed => 23
	i64 6222399776351216807, ; 130: System.Text.Json.dll => 0x565a67a0ffe264a7 => 155
	i64 6284145129771520194, ; 131: System.Reflection.Emit.ILGeneration => 0x5735c4b3610850c2 => 143
	i64 6357457916754632952, ; 132: _Microsoft.Android.Resource.Designer => 0x583a3a4ac2a7a0f8 => 34
	i64 6401687960814735282, ; 133: Xamarin.AndroidX.Lifecycle.LiveData.Core => 0x58d75d486341cfb2 => 69
	i64 6403742896930319886, ; 134: Xamarin.Firebase.Auth.dll => 0x58deaa3c7c766e0e => 83
	i64 6478287442656530074, ; 135: hr\Microsoft.Maui.Controls.resources => 0x59e7801b0c6a8e9a => 11
	i64 6548213210057960872, ; 136: Xamarin.AndroidX.CustomView.dll => 0x5adfed387b066da8 => 65
	i64 6560151584539558821, ; 137: Microsoft.Extensions.Options => 0x5b0a571be53243a5 => 41
	i64 6743165466166707109, ; 138: nl\Microsoft.Maui.Controls.resources => 0x5d948943c08c43a5 => 19
	i64 6777482997383978746, ; 139: pt/Microsoft.Maui.Controls.resources.dll => 0x5e0e74e0a2525efa => 22
	i64 6786606130239981554, ; 140: System.Diagnostics.TraceSource => 0x5e2ede51877147f2 => 118
	i64 6814185388980153342, ; 141: System.Xml.XDocument.dll => 0x5e90d98217d1abfe => 163
	i64 6830730639540541024, ; 142: Plugin.CloudFirestore.dll => 0x5ecba1536e30d660 => 50
	i64 6876862101832370452, ; 143: System.Xml.Linq => 0x5f6f85a57d108914 => 161
	i64 6894844156784520562, ; 144: System.Numerics.Vectors => 0x5faf683aead1ad72 => 138
	i64 7046432119089891704, ; 145: ChibitsLink.dll => 0x61c9f4adc0418978 => 105
	i64 7083547580668757502, ; 146: System.Private.Xml.Linq.dll => 0x624dd0fe8f56c5fe => 141
	i64 7220009545223068405, ; 147: sv/Microsoft.Maui.Controls.resources.dll => 0x6432a06d99f35af5 => 26
	i64 7270811800166795866, ; 148: System.Linq => 0x64e71ccf51a90a5a => 126
	i64 7377312882064240630, ; 149: System.ComponentModel.TypeConverter.dll => 0x66617afac45a2ff6 => 112
	i64 7482377913622462223, ; 150: Xamarin.Protobuf.Lite => 0x67d6bf11b0b1ff0f => 104
	i64 7488575175965059935, ; 151: System.Xml.Linq.dll => 0x67ecc3724534ab5f => 161
	i64 7489048572193775167, ; 152: System.ObjectModel => 0x67ee71ff6b419e3f => 139
	i64 7602111570124318452, ; 153: System.Reactive => 0x698020320025a6f4 => 55
	i64 7654504624184590948, ; 154: System.Net.Http => 0x6a3a4366801b8264 => 128
	i64 7708790323521193081, ; 155: ms/Microsoft.Maui.Controls.resources.dll => 0x6afb1ff4d1730479 => 17
	i64 7714652370974252055, ; 156: System.Private.CoreLib => 0x6b0ff375198b9c17 => 166
	i64 7735176074855944702, ; 157: Microsoft.CSharp => 0x6b58dda848e391fe => 106
	i64 7735352534559001595, ; 158: Xamarin.Kotlin.StdLib.dll => 0x6b597e2582ce8bfb => 102
	i64 7836164640616011524, ; 159: Xamarin.AndroidX.AppCompat.AppCompatResources => 0x6cbfa6390d64d704 => 58
	i64 7991572870742010042, ; 160: Xamarin.Firebase.Firestore.dll => 0x6ee7c52f4d39e8ba => 88
	i64 8064050204834738623, ; 161: System.Collections.dll => 0x6fe942efa61731bf => 110
	i64 8076806894754251516, ; 162: Square.OkHttp => 0x70169513f940c2fc => 53
	i64 8083354569033831015, ; 163: Xamarin.AndroidX.Lifecycle.Common.dll => 0x702dd82730cad267 => 68
	i64 8087206902342787202, ; 164: System.Diagnostics.DiagnosticSource => 0x703b87d46f3aa082 => 117
	i64 8167236081217502503, ; 165: Java.Interop.dll => 0x7157d9f1a9b8fd27 => 167
	i64 8185542183669246576, ; 166: System.Collections => 0x7198e33f4794aa70 => 110
	i64 8246048515196606205, ; 167: Microsoft.Maui.Graphics.dll => 0x726fd96f64ee56fd => 47
	i64 8368701292315763008, ; 168: System.Security.Cryptography => 0x7423997c6fd56140 => 152
	i64 8400357532724379117, ; 169: Xamarin.AndroidX.Navigation.UI.dll => 0x749410ab44503ded => 76
	i64 8410671156615598628, ; 170: System.Reflection.Emit.Lightweight.dll => 0x74b8b4daf4b25224 => 144
	i64 8563666267364444763, ; 171: System.Private.Uri => 0x76d841191140ca5b => 140
	i64 8609060182490045521, ; 172: Square.OkIO.dll => 0x7779869f8b475c51 => 54
	i64 8614108721271900878, ; 173: pt-BR/Microsoft.Maui.Controls.resources.dll => 0x778b763e14018ace => 21
	i64 8626175481042262068, ; 174: Java.Interop => 0x77b654e585b55834 => 167
	i64 8638972117149407195, ; 175: Microsoft.CSharp.dll => 0x77e3cb5e8b31d7db => 106
	i64 8639588376636138208, ; 176: Xamarin.AndroidX.Navigation.Runtime => 0x77e5fbdaa2fda2e0 => 75
	i64 8677882282824630478, ; 177: pt-BR\Microsoft.Maui.Controls.resources => 0x786e07f5766b00ce => 21
	i64 8725526185868997716, ; 178: System.Diagnostics.DiagnosticSource.dll => 0x79174bd613173454 => 117
	i64 8941376889969657626, ; 179: System.Xml.XDocument => 0x7c1626e87187471a => 163
	i64 9045785047181495996, ; 180: zh-HK\Microsoft.Maui.Controls.resources => 0x7d891592e3cb0ebc => 31
	i64 9226675035665529780, ; 181: Xamarin.Protobuf.Lite.dll => 0x800bbc0f56cefbb4 => 104
	i64 9312692141327339315, ; 182: Xamarin.AndroidX.ViewPager2 => 0x813d54296a634f33 => 82
	i64 9324707631942237306, ; 183: Xamarin.AndroidX.AppCompat => 0x8168042fd44a7c7a => 57
	i64 9659729154652888475, ; 184: System.Text.RegularExpressions => 0x860e407c9991dd9b => 156
	i64 9678050649315576968, ; 185: Xamarin.AndroidX.CoordinatorLayout.dll => 0x864f57c9feb18c88 => 62
	i64 9702891218465930390, ; 186: System.Collections.NonGeneric.dll => 0x86a79827b2eb3c96 => 108
	i64 9808709177481450983, ; 187: Mono.Android.dll => 0x881f890734e555e7 => 169
	i64 9933555792566666578, ; 188: System.Linq.Queryable.dll => 0x89db145cf475c552 => 125
	i64 9956195530459977388, ; 189: Microsoft.Maui => 0x8a2b8315b36616ac => 45
	i64 9991543690424095600, ; 190: es/Microsoft.Maui.Controls.resources.dll => 0x8aa9180c89861370 => 6
	i64 10038780035334861115, ; 191: System.Net.Http.dll => 0x8b50e941206af13b => 128
	i64 10051358222726253779, ; 192: System.Private.Xml => 0x8b7d990c97ccccd3 => 142
	i64 10078727084704864206, ; 193: System.Net.WebSockets.Client => 0x8bded4e257f117ce => 136
	i64 10092835686693276772, ; 194: Microsoft.Maui.Controls => 0x8c10f49539bd0c64 => 43
	i64 10143853363526200146, ; 195: da\Microsoft.Maui.Controls.resources => 0x8cc634e3c2a16b52 => 3
	i64 10229024438826829339, ; 196: Xamarin.AndroidX.CustomView => 0x8df4cb880b10061b => 65
	i64 10245369515835430794, ; 197: System.Reflection.Emit.Lightweight => 0x8e2edd4ad7fc978a => 144
	i64 10364469296367737616, ; 198: System.Reflection.Emit.ILGeneration.dll => 0x8fd5fde967711b10 => 143
	i64 10406448008575299332, ; 199: Xamarin.KotlinX.Coroutines.Core.Jvm.dll => 0x906b2153fcb3af04 => 103
	i64 10430153318873392755, ; 200: Xamarin.AndroidX.Core => 0x90bf592ea44f6673 => 63
	i64 10506226065143327199, ; 201: ca\Microsoft.Maui.Controls.resources => 0x91cd9cf11ed169df => 1
	i64 10785150219063592792, ; 202: System.Net.Primitives => 0x95ac8cfb68830758 => 131
	i64 10822644899632537592, ; 203: System.Linq.Queryable => 0x9631c23204ca5ff8 => 125
	i64 10857315922431607327, ; 204: Xamarin.Firebase.ProtoliteWellKnownTypes => 0x96acef4e92ba821f => 89
	i64 10966933586012635777, ; 205: Xamarin.Grpc.OkHttp.dll => 0x98325ffdbd958281 => 97
	i64 11002576679268595294, ; 206: Microsoft.Extensions.Logging.Abstractions => 0x98b1013215cd365e => 40
	i64 11009005086950030778, ; 207: Microsoft.Maui.dll => 0x98c7d7cc621ffdba => 45
	i64 11023048688141570732, ; 208: System.Core => 0x98f9bc61168392ac => 115
	i64 11103970607964515343, ; 209: hu\Microsoft.Maui.Controls.resources => 0x9a193a6fc41a6c0f => 12
	i64 11162124722117608902, ; 210: Xamarin.AndroidX.ViewPager => 0x9ae7d54b986d05c6 => 81
	i64 11220793807500858938, ; 211: ja\Microsoft.Maui.Controls.resources => 0x9bb8448481fdd63a => 15
	i64 11226290749488709958, ; 212: Microsoft.Extensions.Options.dll => 0x9bcbcbf50c874146 => 41
	i64 11299661109949763898, ; 213: Xamarin.AndroidX.Collection.Jvm => 0x9cd075e94cda113a => 61
	i64 11340910727871153756, ; 214: Xamarin.AndroidX.CursorAdapter => 0x9d630238642d465c => 64
	i64 11485890710487134646, ; 215: System.Runtime.InteropServices => 0x9f6614bf0f8b71b6 => 146
	i64 11496466075493495264, ; 216: Xamarin.Grpc.Context.dll => 0x9f8ba6fc1a1e71e0 => 95
	i64 11518296021396496455, ; 217: id\Microsoft.Maui.Controls.resources => 0x9fd9353475222047 => 13
	i64 11529969570048099689, ; 218: Xamarin.AndroidX.ViewPager.dll => 0xa002ae3c4dc7c569 => 81
	i64 11530571088791430846, ; 219: Microsoft.Extensions.Logging => 0xa004d1504ccd66be => 39
	i64 11543422055205009205, ; 220: Xamarin.Firebase.Firestore => 0xa032793314e77735 => 88
	i64 11705530742807338875, ; 221: he/Microsoft.Maui.Controls.resources.dll => 0xa272663128721f7b => 9
	i64 11902137738784770347, ; 222: Xamarin.Io.OpenCensus.OpenCensusContribGrpcMetrics.dll => 0xa52ce3369409892b => 101
	i64 11953878187842654044, ; 223: Plugin.BLE => 0xa5e4b4e0a2a6cf5c => 49
	i64 12145679461940342714, ; 224: System.Text.Json => 0xa88e1f1ebcb62fba => 155
	i64 12201331334810686224, ; 225: System.Runtime.Serialization.Primitives.dll => 0xa953d6341e3bd310 => 150
	i64 12451044538927396471, ; 226: Xamarin.AndroidX.Fragment.dll => 0xaccaff0a2955b677 => 67
	i64 12466513435562512481, ; 227: Xamarin.AndroidX.Loader.dll => 0xad01f3eb52569061 => 72
	i64 12475113361194491050, ; 228: _Microsoft.Android.Resource.Designer.dll => 0xad2081818aba1caa => 34
	i64 12517810545449516888, ; 229: System.Diagnostics.TraceSource.dll => 0xadb8325e6f283f58 => 118
	i64 12538491095302438457, ; 230: Xamarin.AndroidX.CardView.dll => 0xae01ab382ae67e39 => 59
	i64 12550732019250633519, ; 231: System.IO.Compression => 0xae2d28465e8e1b2f => 123
	i64 12681088699309157496, ; 232: it/Microsoft.Maui.Controls.resources.dll => 0xaffc46fc178aec78 => 14
	i64 12700543734426720211, ; 233: Xamarin.AndroidX.Collection => 0xb041653c70d157d3 => 60
	i64 12708922737231849740, ; 234: System.Text.Encoding.Extensions => 0xb05f29e50e96e90c => 153
	i64 12823819093633476069, ; 235: th/Microsoft.Maui.Controls.resources.dll => 0xb1f75b85abe525e5 => 27
	i64 12843321153144804894, ; 236: Microsoft.Extensions.Primitives => 0xb23ca48abd74d61e => 42
	i64 12859557719246324186, ; 237: System.Net.WebHeaderCollection.dll => 0xb276539ce04f41da => 135
	i64 13068258254871114833, ; 238: System.Runtime.Serialization.Formatters.dll => 0xb55bc7a4eaa8b451 => 149
	i64 13084382143907087733, ; 239: Xamarin.Grpc.Context => 0xb595103c610bc575 => 95
	i64 13221551921002590604, ; 240: ca/Microsoft.Maui.Controls.resources.dll => 0xb77c636bdebe318c => 1
	i64 13222659110913276082, ; 241: ja/Microsoft.Maui.Controls.resources.dll => 0xb78052679c1178b2 => 15
	i64 13343850469010654401, ; 242: Mono.Android.Runtime.dll => 0xb92ee14d854f44c1 => 168
	i64 13381594904270902445, ; 243: he\Microsoft.Maui.Controls.resources => 0xb9b4f9aaad3e94ad => 9
	i64 13384245460423520048, ; 244: Plugin.BLE.dll => 0xb9be64555f2daf30 => 49
	i64 13465488254036897740, ; 245: Xamarin.Kotlin.StdLib => 0xbadf06394d106fcc => 102
	i64 13467053111158216594, ; 246: uk/Microsoft.Maui.Controls.resources.dll => 0xbae49573fde79792 => 29
	i64 13540124433173649601, ; 247: vi\Microsoft.Maui.Controls.resources => 0xbbe82f6eede718c1 => 30
	i64 13545416393490209236, ; 248: id/Microsoft.Maui.Controls.resources.dll => 0xbbfafc7174bc99d4 => 13
	i64 13572454107664307259, ; 249: Xamarin.AndroidX.RecyclerView.dll => 0xbc5b0b19d99f543b => 77
	i64 13609095008681508810, ; 250: Xamarin.Grpc.Protobuf.Lite => 0xbcdd37ce6b00bfca => 98
	i64 13689508124566831556, ; 251: Square.OkHttp.dll => 0xbdfae71bf2a141c4 => 53
	i64 13717397318615465333, ; 252: System.ComponentModel.Primitives.dll => 0xbe5dfc2ef2f87d75 => 111
	i64 13755568601956062840, ; 253: fr/Microsoft.Maui.Controls.resources.dll => 0xbee598c36b1b9678 => 8
	i64 13814445057219246765, ; 254: hr/Microsoft.Maui.Controls.resources.dll => 0xbfb6c49664b43aad => 11
	i64 13881769479078963060, ; 255: System.Console.dll => 0xc0a5f3cade5c6774 => 114
	i64 13959074834287824816, ; 256: Xamarin.AndroidX.Fragment => 0xc1b8989a7ad20fb0 => 67
	i64 14100563506285742564, ; 257: da/Microsoft.Maui.Controls.resources.dll => 0xc3af43cd0cff89e4 => 3
	i64 14124974489674258913, ; 258: Xamarin.AndroidX.CardView => 0xc405fd76067d19e1 => 59
	i64 14125464355221830302, ; 259: System.Threading.dll => 0xc407bafdbc707a9e => 160
	i64 14165531176311179688, ; 260: Xamarin.Firebase.Auth => 0xc496138d7abfc9a8 => 83
	i64 14254574811015963973, ; 261: System.Text.Encoding.Extensions.dll => 0xc5d26c4442d66545 => 153
	i64 14382082037123372364, ; 262: Xamarin.Firebase.Auth.Interop => 0xc7976b69c943d54c => 84
	i64 14461014870687870182, ; 263: System.Net.Requests.dll => 0xc8afd8683afdece6 => 132
	i64 14464374589798375073, ; 264: ru\Microsoft.Maui.Controls.resources => 0xc8bbc80dcb1e5ea1 => 24
	i64 14522721392235705434, ; 265: el/Microsoft.Maui.Controls.resources.dll => 0xc98b12295c2cf45a => 5
	i64 14551742072151931844, ; 266: System.Text.Encodings.Web.dll => 0xc9f22c50f1b8fbc4 => 154
	i64 14622043554576106986, ; 267: System.Runtime.Serialization.Formatters => 0xcaebef2458cc85ea => 149
	i64 14669215534098758659, ; 268: Microsoft.Extensions.DependencyInjection.dll => 0xcb9385ceb3993c03 => 37
	i64 14671188939680189912, ; 269: Xamarin.Grpc.Core => 0xcb9a889bfe470dd8 => 96
	i64 14705122255218365489, ; 270: ko\Microsoft.Maui.Controls.resources => 0xcc1316c7b0fb5431 => 16
	i64 14744092281598614090, ; 271: zh-Hans\Microsoft.Maui.Controls.resources => 0xcc9d89d004439a4a => 32
	i64 14789919016435397935, ; 272: Xamarin.Firebase.Common.dll => 0xcd4058fc2f6d352f => 85
	i64 14852515768018889994, ; 273: Xamarin.AndroidX.CursorAdapter.dll => 0xce1ebc6625a76d0a => 64
	i64 14892012299694389861, ; 274: zh-Hant/Microsoft.Maui.Controls.resources.dll => 0xceab0e490a083a65 => 33
	i64 14904040806490515477, ; 275: ar\Microsoft.Maui.Controls.resources => 0xced5ca2604cb2815 => 0
	i64 14954917835170835695, ; 276: Microsoft.Extensions.DependencyInjection.Abstractions.dll => 0xcf8a8a895a82ecef => 38
	i64 14984936317414011727, ; 277: System.Net.WebHeaderCollection => 0xcff5302fe54ff34f => 135
	i64 14987728460634540364, ; 278: System.IO.Compression.dll => 0xcfff1ba06622494c => 123
	i64 15015154896917945444, ; 279: System.Net.Security.dll => 0xd0608bd33642dc64 => 133
	i64 15076659072870671916, ; 280: System.ObjectModel.dll => 0xd13b0d8c1620662c => 139
	i64 15111608613780139878, ; 281: ms\Microsoft.Maui.Controls.resources => 0xd1b737f831192f66 => 17
	i64 15115185479366240210, ; 282: System.IO.Compression.Brotli.dll => 0xd1c3ed1c1bc467d2 => 122
	i64 15133485256822086103, ; 283: System.Linq.dll => 0xd204f0a9127dd9d7 => 126
	i64 15227001540531775957, ; 284: Microsoft.Extensions.Configuration.Abstractions.dll => 0xd3512d3999b8e9d5 => 36
	i64 15370334346939861994, ; 285: Xamarin.AndroidX.Core.dll => 0xd54e65a72c560bea => 63
	i64 15391712275433856905, ; 286: Microsoft.Extensions.DependencyInjection.Abstractions => 0xd59a58c406411f89 => 38
	i64 15527772828719725935, ; 287: System.Console => 0xd77dbb1e38cd3d6f => 114
	i64 15536481058354060254, ; 288: de\Microsoft.Maui.Controls.resources => 0xd79cab34eec75bde => 4
	i64 15557562860424774966, ; 289: System.Net.Sockets => 0xd7e790fe7a6dc536 => 134
	i64 15582737692548360875, ; 290: Xamarin.AndroidX.Lifecycle.ViewModelSavedState => 0xd841015ed86f6aab => 71
	i64 15609085926864131306, ; 291: System.dll => 0xd89e9cf3334914ea => 164
	i64 15661133872274321916, ; 292: System.Xml.ReaderWriter.dll => 0xd9578647d4bfb1fc => 162
	i64 15664356999916475676, ; 293: de/Microsoft.Maui.Controls.resources.dll => 0xd962f9b2b6ecd51c => 4
	i64 15743187114543869802, ; 294: hu/Microsoft.Maui.Controls.resources.dll => 0xda7b09450ae4ef6a => 12
	i64 15783653065526199428, ; 295: el\Microsoft.Maui.Controls.resources => 0xdb0accd674b1c484 => 5
	i64 15788897513097211459, ; 296: Xamarin.Firebase.ProtoliteWellKnownTypes.dll => 0xdb1d6ea28f352e43 => 89
	i64 15847085070278954535, ; 297: System.Threading.Channels.dll => 0xdbec27e8f35f8e27 => 157
	i64 15930129725311349754, ; 298: Xamarin.GooglePlayServices.Tasks.dll => 0xdd1330956f12f3fa => 92
	i64 16018552496348375205, ; 299: System.Net.NetworkInformation.dll => 0xde4d54a020caa8a5 => 130
	i64 16154507427712707110, ; 300: System => 0xe03056ea4e39aa26 => 164
	i64 16219561732052121626, ; 301: System.Net.Security => 0xe1177575db7c781a => 133
	i64 16288847719894691167, ; 302: nb\Microsoft.Maui.Controls.resources => 0xe20d9cb300c12d5f => 18
	i64 16303230644352379770, ; 303: Xamarin.Grpc.OkHttp => 0xe240b5e48fe2eb7a => 97
	i64 16321164108206115771, ; 304: Microsoft.Extensions.Logging.Abstractions.dll => 0xe2806c487e7b0bbb => 40
	i64 16454459195343277943, ; 305: System.Net.NetworkInformation => 0xe459fb756d988f77 => 130
	i64 16649148416072044166, ; 306: Microsoft.Maui.Graphics => 0xe70da84600bb4e86 => 47
	i64 16677317093839702854, ; 307: Xamarin.AndroidX.Navigation.UI => 0xe771bb8960dd8b46 => 76
	i64 16833383113903931215, ; 308: mscorlib => 0xe99c30c1484d7f4f => 165
	i64 16856067890322379635, ; 309: System.Data.Common.dll => 0xe9ecc87060889373 => 116
	i64 16890310621557459193, ; 310: System.Text.RegularExpressions.dll => 0xea66700587f088f9 => 156
	i64 16942731696432749159, ; 311: sk\Microsoft.Maui.Controls.resources => 0xeb20acb622a01a67 => 25
	i64 16998075588627545693, ; 312: Xamarin.AndroidX.Navigation.Fragment => 0xebe54bb02d623e5d => 74
	i64 17008137082415910100, ; 313: System.Collections.NonGeneric => 0xec090a90408c8cd4 => 108
	i64 17031351772568316411, ; 314: Xamarin.AndroidX.Navigation.Common.dll => 0xec5b843380a769fb => 73
	i64 17062143951396181894, ; 315: System.ComponentModel.Primitives => 0xecc8e986518c9786 => 111
	i64 17089008752050867324, ; 316: zh-Hans/Microsoft.Maui.Controls.resources.dll => 0xed285aeb25888c7c => 32
	i64 17118171214553292978, ; 317: System.Threading.Channels => 0xed8ff6060fc420b2 => 157
	i64 17230721278011714856, ; 318: System.Private.Xml.Linq => 0xef1fd1b5c7a72d28 => 141
	i64 17260702271250283638, ; 319: System.Data.Common => 0xef8a5543bba6bc76 => 116
	i64 17338386382517543202, ; 320: System.Net.WebSockets.Client.dll => 0xf09e528d5c6da122 => 136
	i64 17342750010158924305, ; 321: hi\Microsoft.Maui.Controls.resources => 0xf0add33f97ecc211 => 10
	i64 17438153253682247751, ; 322: sk/Microsoft.Maui.Controls.resources.dll => 0xf200c3fe308d7847 => 25
	i64 17509662556995089465, ; 323: System.Net.WebSockets.dll => 0xf2fed1534ea67439 => 137
	i64 17514990004910432069, ; 324: fr\Microsoft.Maui.Controls.resources => 0xf311be9c6f341f45 => 8
	i64 17623389608345532001, ; 325: pl\Microsoft.Maui.Controls.resources => 0xf492db79dfbef661 => 20
	i64 17702523067201099846, ; 326: zh-HK/Microsoft.Maui.Controls.resources.dll => 0xf5abfef008ae1846 => 31
	i64 17704177640604968747, ; 327: Xamarin.AndroidX.Loader => 0xf5b1dfc36cac272b => 72
	i64 17710060891934109755, ; 328: Xamarin.AndroidX.Lifecycle.ViewModel => 0xf5c6c68c9e45303b => 70
	i64 17712670374920797664, ; 329: System.Runtime.InteropServices.dll => 0xf5d00bdc38bd3de0 => 146
	i64 17777860260071588075, ; 330: System.Runtime.Numerics.dll => 0xf6b7a5b72419c0eb => 148
	i64 17986907704309214542, ; 331: Xamarin.GooglePlayServices.Basement.dll => 0xf99e554223166d4e => 91
	i64 18025913125965088385, ; 332: System.Threading => 0xfa28e87b91334681 => 160
	i64 18099568558057551825, ; 333: nl/Microsoft.Maui.Controls.resources.dll => 0xfb2e95b53ad977d1 => 19
	i64 18121036031235206392, ; 334: Xamarin.AndroidX.Navigation.Common => 0xfb7ada42d3d42cf8 => 73
	i64 18146411883821974900, ; 335: System.Formats.Asn1.dll => 0xfbd50176eb22c574 => 121
	i64 18225059387460068507, ; 336: System.Threading.ThreadPool.dll => 0xfcec6af3cff4a49b => 159
	i64 18245806341561545090, ; 337: System.Collections.Concurrent.dll => 0xfd3620327d587182 => 107
	i64 18305135509493619199, ; 338: Xamarin.AndroidX.Navigation.Runtime.dll => 0xfe08e7c2d8c199ff => 75
	i64 18324163916253801303 ; 339: it\Microsoft.Maui.Controls.resources => 0xfe4c81ff0a56ab57 => 14
], align 8

@assembly_image_cache_indices = dso_local local_unnamed_addr constant [340 x i32] [
	i32 42, ; 0
	i32 169, ; 1
	i32 46, ; 2
	i32 124, ; 3
	i32 60, ; 4
	i32 78, ; 5
	i32 61, ; 6
	i32 79, ; 7
	i32 55, ; 8
	i32 101, ; 9
	i32 7, ; 10
	i32 145, ; 11
	i32 52, ; 12
	i32 158, ; 13
	i32 113, ; 14
	i32 10, ; 15
	i32 66, ; 16
	i32 145, ; 17
	i32 90, ; 18
	i32 18, ; 19
	i32 120, ; 20
	i32 94, ; 21
	i32 100, ; 22
	i32 87, ; 23
	i32 74, ; 24
	i32 84, ; 25
	i32 86, ; 26
	i32 93, ; 27
	i32 131, ; 28
	i32 43, ; 29
	i32 168, ; 30
	i32 158, ; 31
	i32 16, ; 32
	i32 58, ; 33
	i32 71, ; 34
	i32 105, ; 35
	i32 48, ; 36
	i32 127, ; 37
	i32 140, ; 38
	i32 57, ; 39
	i32 6, ; 40
	i32 78, ; 41
	i32 119, ; 42
	i32 28, ; 43
	i32 79, ; 44
	i32 44, ; 45
	i32 93, ; 46
	i32 28, ; 47
	i32 70, ; 48
	i32 2, ; 49
	i32 96, ; 50
	i32 20, ; 51
	i32 119, ; 52
	i32 48, ; 53
	i32 66, ; 54
	i32 107, ; 55
	i32 24, ; 56
	i32 69, ; 57
	i32 154, ; 58
	i32 99, ; 59
	i32 62, ; 60
	i32 151, ; 61
	i32 56, ; 62
	i32 27, ; 63
	i32 165, ; 64
	i32 129, ; 65
	i32 37, ; 66
	i32 2, ; 67
	i32 137, ; 68
	i32 51, ; 69
	i32 7, ; 70
	i32 87, ; 71
	i32 90, ; 72
	i32 54, ; 73
	i32 68, ; 74
	i32 138, ; 75
	i32 148, ; 76
	i32 134, ; 77
	i32 103, ; 78
	i32 85, ; 79
	i32 91, ; 80
	i32 46, ; 81
	i32 35, ; 82
	i32 80, ; 83
	i32 166, ; 84
	i32 22, ; 85
	i32 151, ; 86
	i32 51, ; 87
	i32 36, ; 88
	i32 162, ; 89
	i32 35, ; 90
	i32 77, ; 91
	i32 39, ; 92
	i32 44, ; 93
	i32 132, ; 94
	i32 127, ; 95
	i32 150, ; 96
	i32 98, ; 97
	i32 142, ; 98
	i32 33, ; 99
	i32 113, ; 100
	i32 159, ; 101
	i32 124, ; 102
	i32 112, ; 103
	i32 92, ; 104
	i32 30, ; 105
	i32 0, ; 106
	i32 86, ; 107
	i32 56, ; 108
	i32 80, ; 109
	i32 120, ; 110
	i32 129, ; 111
	i32 147, ; 112
	i32 100, ; 113
	i32 109, ; 114
	i32 109, ; 115
	i32 99, ; 116
	i32 147, ; 117
	i32 52, ; 118
	i32 26, ; 119
	i32 115, ; 120
	i32 29, ; 121
	i32 122, ; 122
	i32 152, ; 123
	i32 82, ; 124
	i32 50, ; 125
	i32 121, ; 126
	i32 23, ; 127
	i32 94, ; 128
	i32 23, ; 129
	i32 155, ; 130
	i32 143, ; 131
	i32 34, ; 132
	i32 69, ; 133
	i32 83, ; 134
	i32 11, ; 135
	i32 65, ; 136
	i32 41, ; 137
	i32 19, ; 138
	i32 22, ; 139
	i32 118, ; 140
	i32 163, ; 141
	i32 50, ; 142
	i32 161, ; 143
	i32 138, ; 144
	i32 105, ; 145
	i32 141, ; 146
	i32 26, ; 147
	i32 126, ; 148
	i32 112, ; 149
	i32 104, ; 150
	i32 161, ; 151
	i32 139, ; 152
	i32 55, ; 153
	i32 128, ; 154
	i32 17, ; 155
	i32 166, ; 156
	i32 106, ; 157
	i32 102, ; 158
	i32 58, ; 159
	i32 88, ; 160
	i32 110, ; 161
	i32 53, ; 162
	i32 68, ; 163
	i32 117, ; 164
	i32 167, ; 165
	i32 110, ; 166
	i32 47, ; 167
	i32 152, ; 168
	i32 76, ; 169
	i32 144, ; 170
	i32 140, ; 171
	i32 54, ; 172
	i32 21, ; 173
	i32 167, ; 174
	i32 106, ; 175
	i32 75, ; 176
	i32 21, ; 177
	i32 117, ; 178
	i32 163, ; 179
	i32 31, ; 180
	i32 104, ; 181
	i32 82, ; 182
	i32 57, ; 183
	i32 156, ; 184
	i32 62, ; 185
	i32 108, ; 186
	i32 169, ; 187
	i32 125, ; 188
	i32 45, ; 189
	i32 6, ; 190
	i32 128, ; 191
	i32 142, ; 192
	i32 136, ; 193
	i32 43, ; 194
	i32 3, ; 195
	i32 65, ; 196
	i32 144, ; 197
	i32 143, ; 198
	i32 103, ; 199
	i32 63, ; 200
	i32 1, ; 201
	i32 131, ; 202
	i32 125, ; 203
	i32 89, ; 204
	i32 97, ; 205
	i32 40, ; 206
	i32 45, ; 207
	i32 115, ; 208
	i32 12, ; 209
	i32 81, ; 210
	i32 15, ; 211
	i32 41, ; 212
	i32 61, ; 213
	i32 64, ; 214
	i32 146, ; 215
	i32 95, ; 216
	i32 13, ; 217
	i32 81, ; 218
	i32 39, ; 219
	i32 88, ; 220
	i32 9, ; 221
	i32 101, ; 222
	i32 49, ; 223
	i32 155, ; 224
	i32 150, ; 225
	i32 67, ; 226
	i32 72, ; 227
	i32 34, ; 228
	i32 118, ; 229
	i32 59, ; 230
	i32 123, ; 231
	i32 14, ; 232
	i32 60, ; 233
	i32 153, ; 234
	i32 27, ; 235
	i32 42, ; 236
	i32 135, ; 237
	i32 149, ; 238
	i32 95, ; 239
	i32 1, ; 240
	i32 15, ; 241
	i32 168, ; 242
	i32 9, ; 243
	i32 49, ; 244
	i32 102, ; 245
	i32 29, ; 246
	i32 30, ; 247
	i32 13, ; 248
	i32 77, ; 249
	i32 98, ; 250
	i32 53, ; 251
	i32 111, ; 252
	i32 8, ; 253
	i32 11, ; 254
	i32 114, ; 255
	i32 67, ; 256
	i32 3, ; 257
	i32 59, ; 258
	i32 160, ; 259
	i32 83, ; 260
	i32 153, ; 261
	i32 84, ; 262
	i32 132, ; 263
	i32 24, ; 264
	i32 5, ; 265
	i32 154, ; 266
	i32 149, ; 267
	i32 37, ; 268
	i32 96, ; 269
	i32 16, ; 270
	i32 32, ; 271
	i32 85, ; 272
	i32 64, ; 273
	i32 33, ; 274
	i32 0, ; 275
	i32 38, ; 276
	i32 135, ; 277
	i32 123, ; 278
	i32 133, ; 279
	i32 139, ; 280
	i32 17, ; 281
	i32 122, ; 282
	i32 126, ; 283
	i32 36, ; 284
	i32 63, ; 285
	i32 38, ; 286
	i32 114, ; 287
	i32 4, ; 288
	i32 134, ; 289
	i32 71, ; 290
	i32 164, ; 291
	i32 162, ; 292
	i32 4, ; 293
	i32 12, ; 294
	i32 5, ; 295
	i32 89, ; 296
	i32 157, ; 297
	i32 92, ; 298
	i32 130, ; 299
	i32 164, ; 300
	i32 133, ; 301
	i32 18, ; 302
	i32 97, ; 303
	i32 40, ; 304
	i32 130, ; 305
	i32 47, ; 306
	i32 76, ; 307
	i32 165, ; 308
	i32 116, ; 309
	i32 156, ; 310
	i32 25, ; 311
	i32 74, ; 312
	i32 108, ; 313
	i32 73, ; 314
	i32 111, ; 315
	i32 32, ; 316
	i32 157, ; 317
	i32 141, ; 318
	i32 116, ; 319
	i32 136, ; 320
	i32 10, ; 321
	i32 25, ; 322
	i32 137, ; 323
	i32 8, ; 324
	i32 20, ; 325
	i32 31, ; 326
	i32 72, ; 327
	i32 70, ; 328
	i32 146, ; 329
	i32 148, ; 330
	i32 91, ; 331
	i32 160, ; 332
	i32 19, ; 333
	i32 73, ; 334
	i32 121, ; 335
	i32 159, ; 336
	i32 107, ; 337
	i32 75, ; 338
	i32 14 ; 339
], align 4

@marshal_methods_number_of_classes = dso_local local_unnamed_addr constant i32 0, align 4

@marshal_methods_class_cache = dso_local local_unnamed_addr global [0 x %struct.MarshalMethodsManagedClass] zeroinitializer, align 8

; Names of classes in which marshal methods reside
@mm_class_names = dso_local local_unnamed_addr constant [0 x ptr] zeroinitializer, align 8

@mm_method_names = dso_local local_unnamed_addr constant [1 x %struct.MarshalMethodName] [
	%struct.MarshalMethodName {
		i64 0, ; id 0x0; name: 
		ptr @.MarshalMethodName.0_name; char* name
	} ; 0
], align 8

; get_function_pointer (uint32_t mono_image_index, uint32_t class_index, uint32_t method_token, void*& target_ptr)
@get_function_pointer = internal dso_local unnamed_addr global ptr null, align 8

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
	store ptr %fn, ptr @get_function_pointer, align 8, !tbaa !3
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
attributes #0 = { "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" nofree norecurse nosync nounwind "stack-protector-buffer-size"="8" "target-cpu"="generic" "target-features"="+fix-cortex-a53-835769,+neon,+outline-atomics,+v8a" uwtable willreturn }
attributes #1 = { nofree nounwind }
attributes #2 = { "no-trapping-math"="true" noreturn nounwind "stack-protector-buffer-size"="8" "target-cpu"="generic" "target-features"="+fix-cortex-a53-835769,+neon,+outline-atomics,+v8a" }

; Metadata
!llvm.module.flags = !{!0, !1, !7, !8, !9, !10}
!0 = !{i32 1, !"wchar_size", i32 4}
!1 = !{i32 7, !"PIC Level", i32 2}
!llvm.ident = !{!2}
!2 = !{!"Xamarin.Android remotes/origin/release/8.0.4xx @ 82d8938cf80f6d5fa6c28529ddfbdb753d805ab4"}
!3 = !{!4, !4, i64 0}
!4 = !{!"any pointer", !5, i64 0}
!5 = !{!"omnipotent char", !6, i64 0}
!6 = !{!"Simple C++ TBAA"}
!7 = !{i32 1, !"branch-target-enforcement", i32 0}
!8 = !{i32 1, !"sign-return-address", i32 0}
!9 = !{i32 1, !"sign-return-address-all", i32 0}
!10 = !{i32 1, !"sign-return-address-with-bkey", i32 0}
