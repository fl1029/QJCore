<template>
    <el-col :sm="24" :md="pzoption.mdwidth">
        <i class="iconfont icon-shezhi pull-right widgetset hidden-print" @click.stop="showdig()"></i>
        <i class="iconfont icon-shanchu pull-right widgetdel hidden-print" @click.stop="delWid(pzoption.wigdetcode)"></i>
        <el-tabs :type="childpro.tabtype" style="margin-top:22px" v-model="childpro.TabsValue">
            <el-tab-pane v-for="(item, index) in childpro.Tabs"  :key="item.name"  :label="item.title"  :name="item.name" style="min-height:200px"></el-tab-pane>
        </el-tabs>
        <el-dialog title="组件属性"  :visible.sync="dialogInputVisible">
            <el-form>
                <el-form-item label="Tab样式">
                    <el-radio v-model="childpro.tabtype" label="">样式一</el-radio>
                    <el-radio v-model="childpro.tabtype" label="card">样式二</el-radio>
                    <el-radio v-model="childpro.tabtype" label="border-card">样式三</el-radio>
                </el-form-item>
            </el-form>
            <el-card class="box-card">
                <div slot="header" class="clearfix">
                    <span>页面组件(点击下面组件加入到对应Tab中去)</span>
                </div>
                <div class="item">
                    <el-row>
                        <el-button v-for="(item, index) in tempwigs" size="small" @click="totab(item)" style="margin-top:10px">{{item.wigdetcode}}{{item.title}}</el-button>
                    </el-row>
                </div>
            </el-card>
            <el-button size="small" @click="addTab(childpro.TabsValue)" style="margin-top:10px;display:none">
                添加Table
            </el-button>
            <el-tabs :type="childpro.tabtype" v-model="childpro.TabsValue" style="margin-top:10px" closable @tab-remove="removeTab">
                <el-tab-pane v-for="(item, index) in childpro.Tabs"  :key="item.name"  :label="item.title"  :name="item.name" style="min-height:200px">
                    <el-form>
                        <el-form-item label="Tab名称">
                            <el-input v-model="item.title" autocomplete="off"></el-input>
                        </el-form-item>
                    </el-form>
                    <el-row style="margin-top:10px">
                        <el-button v-for="(el, elindex)  in item.content" size="small" @click="towigs(el,elindex)">{{el.wigdetcode}}{{el.title}}</el-button>
                    </el-row>
                </el-tab-pane>
            </el-tabs>
            <!--<span slot="footer" class="dialog-footer">
                <el-button @click="dialogInputVisible = false">取 消</el-button>
                <el-button type="primary" @click="qd()">确 定</el-button>
            </span>-->

        </el-dialog>
    </el-col>

</template>
<script>
    module.exports = {
        props: ['pzoption', 'index'],
        data: function () {
            return {
                dialogInputVisible: false,
                iscontrols: false,
                tempwigs: [],
                childpro: {
                    placeholder: "区域一",
                    disabled: false,
                    height: [200, 200, 200],
                    tabIndex: 2,
                    tabtype: "border-card",
                    TabsValue: '1',
                    Tabs: [{
                        title: '标签一',
                        name: '1',
                        content: []
                    }, {
                        title: '标签二',
                        name: '2',
                        content: []
                    }, {
                        title: '标签三',
                        name: '3',
                        content: []
                    }]
                }
            }
        },
        methods: {
            delWid: function (wigdetcode) {
                // 子组件中触发父组件方法ee并传值cc12345
                this.$root.nowwidget = {};
                _.remove(this.$root.FormData.wigetitems, function (obj) {
                    return obj.wigdetcode == wigdetcode;
                });
            },
            totab: function (item) {
                var acttabname = this.childpro.TabsValue;
                this.childpro.Tabs.forEach((tab, index) => {
                    _.remove(tab.content, function (obj) {
                        return obj.wigdetcode == item.wigdetcode;
                    });
                });//先删除原有的
                this.childpro.Tabs.forEach((tab, index) => {
                    if (tab.name === acttabname) {
                        tab.content.push(item);
                    }
                });//再添加

            },
            towigs: function (item, elindex) {
                debugger;
                var cho = this;
                var acttabname = cho.childpro.TabsValue;
                cho.childpro.Tabs.forEach((tab, index) => {
                    if (tab.name === acttabname) {
                        tab.content.splice(elindex, 1);
                    }
                });

            },
            qd: function () {

            },
            showdig: function () {
                this.dialogInputVisible = true;
                var cho = this;
                cho.tempwigs = [];
                //this.tempwigs = tabs.filter(tab => tab.name !== targetName);
                _.forEach(this.$root.FormData.wigetitems, function (wig) {
                    if (wig.component != "qjTab") {
                        cho.tempwigs.push({ title: wig.title, wigdetcode: wig.wigdetcode });
                    }
                });

            },
            senddata: function () {
                this.$emit('data-change', JSON.stringify(this.childpro), this.pzoption.wigdetcode);

            },
            addTab(targetName) {
                let newTabName = ++this.childpro.tabIndex + '';
                this.childpro.Tabs.push({
                    title: 'NewTab',
                    name: newTabName,
                    content: []
                });
                this.childpro.TabsValue = newTabName;
            },
            removeTab(targetName) {
                this.$confirm('确定要删除吗?', '提示', {
                    confirmButtonText: '确定',
                    cancelButtonText: '取消',
                    type: 'warning'
                }).then(() => {
                    let tabs = this.childpro.Tabs;
                    let activeName = this.childpro.TabsValue;
                    if (activeName === targetName) {
                        tabs.forEach((tab, index) => {
                            if (tab.name === targetName) {
                                let nextTab = tabs[index + 1] || tabs[index - 1];
                                if (nextTab) {
                                    activeName = nextTab.name;
                                }
                            }
                        });
                    }

                    this.childpro.TabsValue = activeName;
                    this.childpro.Tabs = tabs.filter(tab => tab.name !== targetName);
                }).catch(() => {
                    this.$message({
                        type: 'info',
                        message: '已取消删除'
                    });
                });
              
            }
        },
        mounted: function () {
            var pro = this;
            pro.$nextTick(function () {
                if (pro.$root.addchildwig) {
                    pro.$root.addchildwig();//不能缺少,dom加载完成
                }
                if (pro.pzoption.childpro.TabsValue) {
                    pro.childpro = pro.pzoption.childpro;
                } else {
                    pro.senddata();
                }
            })

        },
        watch: {
            childpro: { //深度监听，可监听到对象、数组的变化
                handler(newV, oldV) {
                    this.senddata();
                },
                deep: true
            }
        }
    };
</script>