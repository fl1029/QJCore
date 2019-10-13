<template>
    <el-col :sm="24" :md="pzoption.mdwidth">
        <i class="iconfont icon-shezhi pull-right widgetset hidden-print" @click.stop="dialogInputVisible = true"></i>
        <i class="iconfont icon-shanchu pull-right widgetdel hidden-print" @click.stop="delWid(pzoption.wigdetcode)"></i>
        <el-form-item :label="pzoption.title" :prop="'wigetitems.' + index + '.value'" :rules="childpro.rules">
            <el-input-number :placeholder="childpro.placeholder" v-model="pzoption.value" :disabled="childpro.disabled" controls-position="right" :max="childpro.max" :min="childpro.min" :precision="childpro.precision">
            </el-input-number>
        </el-form-item>
        <el-dialog title="组件属性" :visible.sync="dialogInputVisible">
            <el-tabs type="border-card">
                <el-tab-pane label="个性配置">
                    <el-form :model="childpro">
                        <el-form-item label="占位符">
                            <el-input v-model="childpro.placeholder" autocomplete="off"></el-input>
                        </el-form-item>
                        <el-form-item label="必填">
                            <el-switch v-model="childpro.rules.required"></el-switch>
                        </el-form-item>
                        <el-form-item label="只读">
                            <el-switch v-model="childpro.disabled"></el-switch>
                        </el-form-item>
                        <el-form-item label="数字精度(小数点后位数)">
                            <el-input-number v-model="childpro.precision" :controls="iscontrols"></el-input-number>

                        </el-form-item>
                        <el-form-item label="最大数">
                            <el-input-number v-model="childpro.max" :controls="iscontrols"></el-input-number>

                        </el-form-item>
                        <el-form-item label="最小数">
                            <el-input-number v-model="childpro.min" :controls="iscontrols"></el-input-number>

                        </el-form-item>
                    </el-form>
                </el-tab-pane>
                <!--<el-tab-pane label="配置管理">配置管理</el-tab-pane>-->
    
            </el-tabs>
            
            <!--<div slot="footer" class="dialog-footer">
                <el-button type="primary" @click="dialogInputVisible = false">关闭</el-button>
            </div>-->
        </el-dialog>
    </el-col>

</template>
<script>
    module.exports = {
        props: ['pzoption', 'index'],
        data: function () {
            return {
                dialogInputVisible: false,
                iscontrols:false,
                childpro: {
                    placeholder: "",
                    disabled: false,
                    min:0,
                    max: 10000,
                    precision:0,
                    rules: {
                        required: true, message: '不能为空', type: "number",
                    }
                }
            }
        },
        methods: {
            delWid: function (wigdetcode) {
                this.$root.nowwidget = {};
                _.remove(this.$root.FormData.wigetitems, function (obj) {
                    return obj.wigdetcode == wigdetcode;
                });
            },
            senddata: function () {
                this.$emit('data-change', JSON.stringify(this.childpro), this.pzoption.wigdetcode);
            }
        },
        mounted: function () {
            var pro = this;
            pro.$nextTick(function () {
                if (pro.$root.addchildwig) {
                    pro.$root.addchildwig();
                }
                if (pro.pzoption.childpro.precision) {
                    pro.childpro = pro.pzoption.childpro
                } else {
                    pro.senddata();
                }
            })

        },
        watch: {
            childpro: {
                handler(newV, oldV) {
                    this.senddata();
                },
                deep: true
            }
        }
    };
</script>