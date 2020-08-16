## [插件名称]
自定义配件

## [插件作者]
#### xiaoye97
- QQ: 1066666683
- Discord: xiaoye#3171
- 开黑啦: 宵夜#2084

## [适用范围]
插件基于PC Building Simulator v1.8.6 steam版开发，只要游戏机制不大概，理论兼容其他平台和后续版本

## [免责声明]
如果使用此插件造成存档损坏，插件作者概不负责!
请您自己承担风险使用

## [注意事项]
请勿将本插件用于任何盈利行为!(如贩卖、加在其他内容中如整合包中贩卖等)

## [安装方法]
1. 打开游戏根目录(从steam右键游戏，选择 管理->浏览本地文件)
2. 备份存档，将游戏根目录下Saves文件夹复制到其他地方备份
3. 安装Bepinex框架
    1. 下载地址(选择BepInEx_x64版本) https://github.com/BepInEx/BepInEx/releases
    2. 将压缩包内的BepInEx文件夹，doorstop_config.ini，winhttp.dll放置在游戏根目录
    3. 运行一次游戏来让框架生成其他文件夹和配置文件
4. 安装插件
    1. 将插件解压，将CustomPart.dll放置到PC Building Simulator\BepInEx\plugins文件夹下
    2. 插件附带一个配件MOD示例，不放入也没关系，可以参考此示例制作自己的配件MOD

## [插件使用方法]
1. 使用别人分享的配件文件
    安装号插件并运行一次之后，游戏根目录会生成CustomPart文件夹，该文件夹下会生成所有可自定义的配件文件夹，将别人分享的txt文件放置到作者指定的文件夹内即可。
2. 制作配件MOD方法
    1. 下载AssetStudio https://github.com/Perfare/AssetStudio/releases
    2. 打开AssetStudio，选择File->Load folder，然后选择游戏根目录确定
    3. 跳转到AssetList页面，在顶部工具栏的Filter Type勾选Text Asset。(只显示文本资源方便查找)
    4. 以自定义内存为例，找到RAM资源，这里存储了所有内存的信息，以此为模板书写txt文件，具体转换方式参考MOD示例文件
    5. 将写好的txt放入对应文件夹，打开游戏之前，记得检查txt是否有拼写错误等，并且记得备份存档
3. Mod导致的存档错误
    如果使用了MOD，但是直接移除了插件或者换了电脑玩，会出现存档错误的提示
4. 正确移除MOD
    1. 确保插件安装正确
    2. 确保存档内所有电脑上的MOD配件已经移除，职业模式等需要将库存里的MOD配件也卖掉
    3. 将MOD配件的txt文件删除(或更改后缀名)
    4. 重启游戏并加载存档，插件会删除之前的配件MOD引用
    5. 如果要删除插件，确保已经移除所有配件MOD

## [鸣谢]
- 感谢Perfare制作的AssetStudio https://github.com/Perfare/AssetStudio
- 感谢bbepis、denikson、ManlyMarco等人制作的BepInEx框架 https://github.com/BepInEx/BepInEx

## [更新历史]
- v1.0 发布