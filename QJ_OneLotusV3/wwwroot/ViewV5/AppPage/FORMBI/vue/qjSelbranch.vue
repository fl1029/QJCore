﻿<template>
    <el-col :sm="24" :md="pzoption.mdwidth">
        <i class="iconfont icon-shezhi pull-right widgetset hidden-print" @click.stop="dialogInputVisible = true"></i>
        <i class="iconfont icon-shanchu pull-right widgetdel hidden-print" @click.stop="delWid(pzoption.wigdetcode)"></i>
        <el-form-item :label="pzoption.title" :prop="'wigetitems.' + index + '.value'" :rules="childpro.rules">
            <el-input v-model="pzoption.value" style="display:none">
            </el-input>
            <el-input :placeholder="childpro.placeholder" v-model="pzoption.valuetext" :disabled="childpro.disabled" readonly >
                <el-button slot="append" v-on:click.native="showWinUser" v-if="!childpro.disabled&&!childpro.readonly"> <i class="iconfont icon-lcsp"></i></el-button>
            </el-input>
            <el-dialog title="提示"
                       :visible.sync="dialogVisible"
                       class="mobeldiog"
                       :before-close="handleClose">
                <iframe :src="'/ViewV5/AppPage/XTGL/BranchSelect.html?isqx=Y&issignle='+childpro.issignle" width="100%" height="400" frameborder="0" :id="iframeid"></iframe>
                <span slot="footer" class="dialog-footer">
                    <el-button @click="dialogVisible = false">取 消</el-button>
                    <el-button type="primary" @click="SelBranch()">确 定</el-button>
                </span>
            </el-dialog>
        </el-form-item>
        <el-dialog title="组件属性" :visible.sync="dialogInputVisible">
            <el-form :model="childpro">
                <el-form-item label="占位符">
                    <el-input v-model="childpro.placeholder" autocomplete="off"></el-input>
                </el-form-item>
                <el-form-item label="必填">
                    <el-switch v-model="childpro.rules.required"></el-switch>
                </el-form-item>
                <el-form-item label="只读">
                    <el-switch v-model="childpro.readonly"></el-switch>
                </el-form-item>
                <el-form-item label="多选">
                    <el-switch v-model="childpro.issignle"  active-value="N" inactive-value="Y"></el-switch>
                </el-form-item>
                <el-form-item label="默认为当前用户所在部门">
                    <el-switch v-model="childpro.ishasdefault"></el-switch>
                </el-form-item>

            </el-form>
        </el-dialog>
    </el-col>
</template>
<script>
    module.exports = {
        props: ['pzoption', 'index'],
        data() {
            return {
                dialogVisible: false,
                iframeid: "",
                dialogInputVisible: false,
                childpro: {
                    placeholder: "",
                    disabled: false,
                    readonly:false,
                    itemtype: "text",
                    issignle: "Y",
                    ishasdefault: false,
                    rules: {
                        required: false, message: '不能为空', type: "string"
                    }
                }
            };
        },
        methods: {
            genID: function () {
                var random = Math.floor(Math.random() * (90000 - 10000 + 1) + 10000);
                return random;
            },
            SelBranch: function () {
                debugger;
                this.dialogVisible = false;
                var branch = document.getElementById(this.iframeid).contentWindow.getselectedbranch();
                this.pzoption.value = branch.branchid;
                this.pzoption.valuetext = branch.branchname;
                
            },
            handleClose: function () {
                this.dialogVisible = false;
            },
            showWinUser() {
                this.dialogVisible = true;
            },
            delWid: function (wigdetcode) {
                // 子组件中触发父组件方法ee并传值cc12345
                this.$root.nowwidget = {};
                _.remove(this.$root.FormData.wigetitems, function (obj) {
                    return obj.wigdetcode == wigdetcode;
                });
            },
            senddata: function () {
                this.$emit('data-change', JSON.stringify(this.childpro), this.pzoption.wigdetcode);
                //配置项变更时传递数据
            }

        },
        created() {
            this.iframeid = this.genID();
        },
        mounted: function () {
            var pro = this;
            pro.$nextTick(function () {
                if (pro.$root.addchildwig) {
                    pro.$root.addchildwig();//不能缺少,dom加载完成
                }
                if (pro.pzoption.childpro.itemtype) {
                    pro.childpro = pro.pzoption.childpro;
                    if (pro.childpro.ishasdefault && app.isview) {
                        var userinfo = ComFunJS.getCookie("userinfo").split(",");
                        pro.pzoption.value = userinfo[2];
                        pro.pzoption.valuetext = userinfo[3];
                    }
                } else {
                    //没有初始化配置时使用默认配置
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

