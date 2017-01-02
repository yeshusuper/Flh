	var illimitedSelect=new Object({
		Data:"",
		Container:"",
		inputName:"",
		ParentNo:4,
		getData:function(v_no){
			var _this=illimitedSelect;
			$.ajax({
				url: "http://admin.t.fuliaohui.com/CommonData/Area?parent=0001&deep=2",
				type: "get",
				dataType: "jsonp",
				success: function (res) {
					alert(res)
					if(res.data){
						_this.Data=res.data.item;
						_this.generateLayout(_this.Data);
						_this.condition(v_no);
					}else{
						console.log(res.msg);
					}
				}
			});
		},
		condition:function(v_no){
			var _this=illimitedSelect,
				levelData=_this.Data;
			if(!levelData &&levelData==""){
				_this.getData(v_no);
			}else{
				if(v_no.length>=_this.ParentNo){

					var floor_num=parseInt(v_no.length/_this.ParentNo);

					for(var i=0;i<floor_num;i++){
						var int_no=v_no.substr(0,_this.ParentNo*(i+1));
						$.each(levelData,function(key){

							if(levelData[key].ClassNo==int_no){

								if(levelData[key].Subs!=""){

									levelData=levelData[key].Subs;
									var s_num=$('select',_this.Container).length;

									if(i+1==s_num){

										_this.generateLayout(levelData);

									}

									return false;
								}

							}

						});
						$('[value="'+int_no+'"]').prop("selected",true);
					}
				}
			}
		},
		generateLayout:function(_data){
			var _this=illimitedSelect,
				html='<option value="">请选择</option>';
			$.each(_data,function(key){
				html+='<option value="'+_data[key].ClassNo+'">'+_data[key].ClassName+'</option>';

			})
			_this.Container.append('<select>'+html+'</select>');
			_this.Featureschange();
		},
		Featureschange:function(){
			var _this=illimitedSelect;
			$('select',_this.Container).on('change',function(){
				var v_no=$(this).val();
				$(this).nextAll().remove();
				if(v_no!=""){
					_this.condition(v_no);
				}
				_this.inputName.val(v_no);
			})
		},
		val:function(v_val){
			var _this=illimitedSelect;
			_this.condition(v_val);
		},
		init:function(v_class,v_input,v_val,parent){
			var _this=illimitedSelect;
			_this.Container=$(v_class);
			_this.inputName=$('[name="'+v_input+'"]');
			if(parent){
				_this.ParentNo=parent;
			}
			if(v_val){
				_this.inputName.val(v_val);
			}
			_this.condition(v_val);
		}
	})


