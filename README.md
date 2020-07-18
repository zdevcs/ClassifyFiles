# 文件分类浏览器

## 介绍



## 截图

![image](https://github.com/autodotua/ClassifyFiles/blob/master/Image/Screenshot_1.png)

![image](https://github.com/autodotua/ClassifyFiles/blob/master/Image/Screenshot_2.png)

![image](https://github.com/autodotua/ClassifyFiles/blob/master/Image/Screenshot_3.png)

![image](https://github.com/autodotua/ClassifyFiles/blob/master/Image/Screenshot_4.png)

![image](https://github.com/autodotua/ClassifyFiles/blob/master/Image/Screenshot_5.png)

![image](https://github.com/autodotua/ClassifyFiles/blob/master/Image/Screenshot_6.png)

## 特性

- 使用.Net Core 3.1 + WPF + SQLite+EFCore 开发
- 使用WinUI风格的界面

## 更新计划和待解决BUG



## 更新日志

### 2020-05-10

开始项目

由《网页内容变动提醒》进行拆除

### 2020-05-11

完成主界面的框架

完成分类树形图的逻辑和显示

基本完成分类设置的匹配条件设置

### 2020-05-12

基本完成了分类树状图的新增和删除

完成分类的重命名

基本完成对文件的枚举分类工具方法

完成文件浏览界面基本功能，包括项目的名称显示、根目录设置

基本完成使用列表视图和图标视图来显示文件。

 ### 2020-05-13

基本完成使用图标视图时的内存分页（图标视图应该没有虚拟化，很卡）

新增树状视图

基本完成对图片缩略图的显示

### 2020-05-14

将图标和缩略图封装为组件，并为三个视图都应用了该组件

新增列表视图支持滚轮缩放

修改了分页按钮样式

新增项目的新建

新增右键打开目录菜单

列表视图新增组合框，用于跳转到指定目录的文件位置

新增Ctrl+滚轮实现图标视图的缩放功能

重写了缩略图策略，改为在枚举文件时就获取缩略图，并使用数据库BLOB保存缩略图

过滤方式新增正则表达式相关、文件大小限制、文件修改时间限制

------

### 2020-05-26

将控件库从MaterialDesign改为ModernWpf，基本完成主要修改

### 2020-05-27

完全清除了MaterialDesign库

新增对于没有缩略图的文件，根据文件类型提供不同的图标，对于文件夹提供文件夹图标

新增了单独的项目设置页

支持了图标视图根据文件夹跳转

修改了一些逻辑

为处理中Ring的覆盖层加了动画

### 2020-05-28

新增了导出为快捷方式或副本的功能

修复了一些BUG，优化了一些功能

### 2020-05-28

支持了导入和导出数据库（项目）的功能

支持了一键删除所有项目

### 2020-06-01

修复了项目设置界面的布局错误

新增可以使用按钮+对话框选择根目录

修改文件浏览界面的左侧分类列表为Expander

新增错误提示框

支持了对单个类进行刷新

### 2020-07-02

为了增加标签模式，大幅度修改了代码，对某些类和界面进行了抽象，新增了一些父类，对文件浏览块进行了封装

修改Class，从树状结构修改为顺序结构

### 20200703

标签类型的显示和管理有了一个样子，可以勉强使用了。基本完成了显示和设置界面。

新增了启动界面，因为主界面打开太慢了。

-------

### 20200703

越写越乱，于是决定再次“重构”。合并了Class和Tag，修改了不同Class同一个文件使用同一个File的设定。
一个物理文件使用一个File，File和Class使用FileClass类进行多对多连接。

没想到，一个下午+晚上，竟然把程序改到能打开并且基本功能正常的地步了。

### 20200704

修复了新建文件时，File的Dir会从\起头的BUG

修复了更新文件的一些BUG，比如文件会重复等

新增删除项目所有文件的功能

项目设置界面新增数据库记录数量的显示

新增显示全部文件按钮

修复了在项目之间快速切换会同时访问数据库导致出错的BUG（实质是Base类中记忆的问题）

修复了无法双击打开文件的BUG（在新建UIFile的时候，没有赋值Project）

列表支持了多选

新增通过右键菜单来选择和取消选择文件的类别（新增了FileClass的Disabled字段，保证重新刷新以后仍然不会被加入该类）

优化图标显示，图标大小将和缩略图大小一样支持自动缩放

修复了将文件添加到类时，会造成一个项目中多个文件对象、一个类中多个文件对象的BUG

新增在标签上按鼠标中键删除标签的功能

### 20200705

新增了更新文件对话框

将配置文件改到了数据库里

将DbUtility分离为了多个类

修复了“根据文件夹定位”不可用的BUG

新增了文件搜索框

修复了之前因数据库大改而失效的导入导出功能

修复了可能无法从类中移除文件的BUG（但至今还是不知道为什么会有多个FIleClass对应同一个File和Class）（后来知道了是我犯了一个SB错误）

### 20200706

修复了可能无法从类中移除文件的BUG（

对分类设置的用户操作做了优化，增删改之后会选中分类

输入对话框弹出后将自动获取焦点并全选

新增对添加文件夹到类的支持，并同步支持了File承载文件夹

新增了新增文件的对话框

### 20200708

新增支持在浏览是尝试自动生成缩略图

优化了文件浏览加载速度，将标签的获取修改为动态获取，大大加快了速度

新增删除项目所有缩略图功能

新增压缩数据库功能

### 20200711

新增平铺视图

修复了树状图中无法在标签上按中键的BUG

启动页面支持了根据亮色/暗色模式自动切换启动图

修复了图标视图的图标不会跟着滚轮放大缩小的BUG

### 20200712

对图标视图和平铺视图启用了虚拟化，大大加快了速度，因此取消了分页

文件浏览页面修改为ContentControl+Resouce

花了一个上午修改了居多东西，大多数是为了适配虚拟化的网格视图，不一一列举了。

### 20200713

优化了动态更新，在新的更新开始时，将会中止已经存在的更新。

新增支持了拖放文件到外部（例如资源管理器），并优化修改了部分列表的选择策略

新增支持了视频缩略图

修改缩略图的存储方式，由数据库改为文件存储，同时修改了数据库字段

修复了图片图标错误的BUG

### 20200714

新增显示资源管理器里显示的图标的功能

修复了右键菜单选择和取消选择类别失效的BUG

新增四个开关：缩略图、标签、资源管理器图标、图标视图的文件名

### 20200715

重构了UIFile，不再继承File，而是单独让File变为一个属性，同时抽象出了UIFileDisplay和UIFileSize两个类

重写了Configs类，使其更方便添加新的配置项

新增了项目、类、视图的记忆功能

新增列表显示文件大小功能

统一了列表和树状图的ItemTemplate

### 20200716

修改了实时获取缩略图的策略。由于发现FineIcon只会在需要显示时初始化，因此干脆把所有的刷新的东西写到了FileIcon中。

新增了全局错误捕捉（暂无GUI提示）

在进行耗时操作（转圈圈）时，将无法关闭窗体

支持了调整最大线程数

支持了更新文件和添加文件时生成资源管理器图标

修复了树状图文件夹在显示资源管理器图标时无法正常显示的BUG

新增支持设置平铺视图是否显示文件夹目录

调整了视图设置弹出面板的布局，并新增了缩放滑块

### 20200717

新增详细信息视图

将大小图标改为可以无级调节的Scale属性，并支持了定死大小的图标

为所有的视图添加了ToolTip

对工具类进行了分离

在获取缩略图时，将显示小小的圈圈转啊转

修复和优化了一些小问题

修复了不把鼠标点在图片上就没法拖放文件的BUG

新增右键菜单复制功能

### 20200718

新增根据目录分组显示功能

修复了根据目录进行定位/定位文件不可用的BUG

新增排序功能

新增日志功能