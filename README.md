ScrollContainer

基于原生ScrollRect的一套滚动列表实现。

## 功能支持:

1. 支持4中单元格容器回调(OnShow--单元格显示时 OnMoveToIndex--单元格滚动或矫正到指定单元格时 OnVisibleScroll--单元格滚动过程中回调 OnHide--单元格隐藏或销毁回调)
2. 滚动到指定单元格显示(支持滚动到指定位置)

3. 动态添加(支持任何位置添加元素)
4. 不同大小单元格显示(支持自适应大小或者自定义传单元格大小)
5. 多种列表类型显示(e.g. 横向,竖向，横向网格，竖向网格等)
6. 不同单元格朝向(e.g. 从左往右或从右往左。从上往下或者从下往上。)
7. 单元格矫正(最终单元格会滚动到某个最近的单元格位置)
8. 单元格对象池(e.g. 单元格数据逻辑对象(Object)对象池。单元格实体对象(GameObject)对象池。)
9. 单元格嵌套滚动(e.g. 嵌套不同方向的单元格容器滚动支持)
10. 本地单元格模拟创建查看效果(e.g. Inspector支持快速模拟查看排版等)

## 效果展示

![MainMenu](/Images/MainMenu.PNG)

![TopToBottomScene](/Images/TopToBottomScene.PNG)

![LeftToRightScene](/Images/LeftToRightScene.PNG)

![GridViewScene](/Images/GridViewScene.PNG)

![ChatMessageListScene](/Images/ChatMessageListScene.PNG)

![ChangeItemSizeScene](/Images/ChangeItemSizeScene.PNG)

![ClickAndLoadMoreScene](/Images/ClickAndLoadMoreScene.PNG)

![HorizontalGalleryDemoScene](/Images/HorizontalGalleryDemoScene.PNG)

![PageViewScene](/Images/PageViewScene.PNG)

![SpinDatePickerScene](/Images/SpinDatePickerScene.PNG)

![SelectAndDeleteOrMoveScene](/Images/SelectAndDeleteOrMoveScene.PNG)

## 实现原理

1. 通过手动传单元格大小和预制件索引来实现自定义单元格大小和预制件选择

2. 通过累加单元格大小计算出总的可滚动Content大小，同时计算出单元格的抽象单元格位置

3. 结合IBeginDragHandler, IDragHandler, IEndDragHandler接口来判定拖拽状态(比如是否开始或结束拖拽，拖拽方向等)，以及实现嵌套单元格的拖拽事件判定以及分发

4. Content结合ScrollRect来实现滚动回调(OnValueChanged)触发滚动计算判定，结合单元格的抽象位置来判定每个单元格的显隐状态。同时判定是否满足单元格矫正条件

5. 通过IEndDragHandler响应结束拖拽时判定是否需要执行单元格矫正

## 优缺点分析

**亮点分析:**

1. 支持嵌套单元格滚动(通过判定滚动方向手动向上传递滚动事件实现)
2. 支持手动自定义Size
3. 支持选择不同单元格模板

**缺点分析:**

1. 滚动单元格依赖于ScrollRect的滚动机制需要一开始就计算出总的单元格大小之和，导致很难支持循环列表。
2. 所有单元格一开始都需要知道大小而不是用到了才请求，导致初始创建动态大小单元格计算单元格大小开销会比较大，同时单元格的动态大小变化一定会影响总的可滚动大小。

## 注意事项

1. 仅横向和竖向单元格支持指定不同单元格大小，网格单元格不支持不同大小的单元格

# 博客链接

[Unity滚动列表](http://tonytang1990.github.io/2020/06/08/Unity滚动列表/)

# 友情链接

[LoopScrollRect](https://github.com/qiankanglai/LoopScrollRect)

[FancyScrollView](https://github.com/setchi/FancyScrollView)

[ugui-super-scrollview-example](https://github.com/baba-s/ugui-super-scrollview-example)