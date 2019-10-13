<template>
    <el-col :sm="24" :md="pzoption.mdwidth">
        <i class="iconfont icon-shezhi pull-right widgetset hidden-print" @click.stop="dialogInputVisible = true"></i>
        <i class="iconfont icon-shanchu pull-right widgetdel hidden-print" @click.stop="delWid(pzoption.wigdetcode)"></i>
        <el-form-item :label="pzoption.title" :prop="'wigetitems.' + index + '.value'" :rules="childpro.rules">
            <el-input :placeholder="childpro.placeholder" :type="childpro.itemtype" :disabled="childpro.disabled"  :readonly="childpro.readonly" v-model="pzoption.value" clearable>
            </el-input>
        </el-form-item>
        <el-dialog title="组件属性" :visible.sync="dialogInputVisible">
            <el-form :model="childpro">
                <el-form-item label="输入框类型">
                    <el-radio v-model="childpro.itemtype" label="text">普通文本</el-radio>
                    <el-radio v-model="childpro.itemtype" label="textarea">多行文本</el-radio>
                </el-form-item>
                <el-form-item label="占位符">
                    <el-input v-model="childpro.placeholder" autocomplete="off"></el-input>
                </el-form-item>
                <el-form-item label="必填">
                    <el-switch v-model="childpro.rules.required"></el-switch>
                </el-form-item>
                <el-form-item label="只读">
                    <el-switch v-model="childpro.readonly"></el-switch>
                </el-form-item>

            </el-form>
        </el-dialog>
    </el-col>

</template>
<script>
    module.exports = {
        props: ['pzoption', 'index'],
        data: function () {
            return {
                dialogInputVisible: false,
                childpro: {
                    placeholder: "区域一",
                    disabled: false,
                    readonly:false,
                    itemtype: "text",
                    rules: {
                        required: false, message: '不能为空', type: "string"
                    }
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
                if (pro.pzoption.childpro.itemtype) {
                    pro.childpro = pro.pzoption.childpro;
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