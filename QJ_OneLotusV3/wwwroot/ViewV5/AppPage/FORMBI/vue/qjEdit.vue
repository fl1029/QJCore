<template>
    <el-col :sm="24" :md="pzoption.mdwidth">
        <i class="iconfont icon-shezhi pull-right widgetset hidden-print" @click.stop="dialogInputVisible = true"></i>
        <i class="iconfont icon-shanchu pull-right widgetdel hidden-print" @click.stop="delWid(pzoption.wigdetcode)"></i>
        <el-form-item :label="pzoption.title" :prop="'wigetitems.' + index + '.value'" :rules="childpro.rules">
            <el-input v-model="pzoption.value" style="display:none">
            </el-input>
            <textarea :id="qjeditid"  class="szhl_UEEDIT"  v-model="pzoption.value"  umheight="400" ></textarea>
        </el-form-item>
        <el-dialog title="组件属性" :visible.sync="dialogInputVisible">
            <el-form :model="childpro">
            
                <el-form-item label="占位符" style="display:none">
                    <el-input v-model="childpro.placeholder" autocomplete="off"></el-input>
                </el-form-item>
                <el-form-item label="必填">
                    <el-switch v-model="childpro.rules.required"></el-switch>
                </el-form-item>
                <el-form-item label="只读">
                    <el-switch v-model="childpro.disabled"></el-switch>
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
                    qjeditid: "",
                    placeholder: "编辑区域",
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
                var um = UE.getEditor(this.qjeditid);
                um.destroy();
                _.remove(this.$root.FormData.wigetitems, function (obj) {
                    return obj.wigdetcode == wigdetcode;
                });
               
            },
            senddata: function () {
                this.$emit('data-change', JSON.stringify(this.childpro), this.pzoption.wigdetcode);

            },
            initedit: function () {
                var pro = this;
                var um = UE.getEditor(pro.qjeditid);
                um.ready(function () {
                    if (pro.pzoption.value != "") {
                        //需要ready后执行，否则可能报错
                        um.setContent(pro.pzoption.value);
                    }
                })
                um.addListener('contentChange', function () {
                    pro.pzoption.value = um.getContent();
                })
                um.addListener('blur', function () {
                    pro.pzoption.value = um.getContent();
                })

            }
        },
        created() {
            this.qjeditid = "edit" + this.pzoption.wigdetcode;
        },
        mounted: function () {
            var pro = this;
            pro.$nextTick(function () {
                if (pro.$root.addchildwig) {
                    pro.$root.addchildwig();
                }
                if (this.pzoption.childpro.placeholder) {
                    pro.childpro = pro.pzoption.childpro;
                } else {
                    pro.senddata();
                }
                pro.initedit();
            })

        },
        watch: {
            childpro: { 
                handler(newV, oldV) {
                    this.senddata();
                },
                deep: true
            },
            "childpro.disabled": {
                handler(newV, oldV) {
                    debugger;
                    var um = UE.getEditor(this.qjeditid);
                        if (um.id) {
                            if (newV) {
                                um.setDisabled();
                            } else {
                                um.setEnabled();
                            }
                        }
                   
                },
                deep: true
            }
        }
    };
</script>