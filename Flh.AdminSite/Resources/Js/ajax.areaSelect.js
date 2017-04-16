(function(window){
	var areaSelector=function(obj){
		 var _this=this,
			 urlParameter;
	     this.idName=$('#'+obj.id);         //������ID21
	     this.className=$('.'+obj.class);   //����������
	     this.modeNum=obj.areaType || 0;         //ģʽ
		 this.areaData="";                  //��������Դ
		 this.cityAreaData="";
		 this._currentNo = '';
		 this.isReady = false;              //�Ƿ��Ѽ���������Դ
		 this._val=obj.defaultVal || "";
		 this._change="";
		 this.issearch="";
		 if( this.areaData &&  this.areaData !=""){
			 //��������Դ
		 }else{
		 	if(userAreaData!=""){
		 		_this.isReady=true;
				_this.areaData=eval("(" + userAreaData+ ")");
				_this._createSelect();
				if(_this._change){
					$("select",_this.idName).on("change",_this._change);
				}
				if(_this._val){
					_this.val(_this._val);
				}
		 	}else{
		 		if(this.modeNum==1){         //ʡ
				 urlParameter={"deep":"1","parent":"0001"};
			 }else if(this.modeNum==2){   //ʡ��
				 urlParameter={"deep":"2","parent":"0001"};
			 }else if(this.modeNum==3){   //ʡ����
				 urlParameter={"deep":"3","parent":"0001"};
			 }else{                 //����
				 urlParameter="";
			 }

			 $.ajax({
				 url:"http://api.tgnet.com/CommonData/Area",
				 type:"GET",
				 data:urlParameter,
				 dataType:"jsonp",
				 success:function(data){
					 if(data){
						 _this.isReady=true;
						 _this.areaData=data;
						 _this._createSelect();
						 if(_this._change){
							 $("select",_this.idName).on("change",_this._change);
						 }
						 if(_this._val){
							 _this.val(_this._val);
						 }
					 }else{
						 console.log("ѡ�����������س���")
					 }
				 }
			 })
		 	}
			 
		 }
	 }

	//����ѡ��������ݳ�ʼ��
	areaSelector.prototype._createSelect=function(){
		var html_s=$("<select/>"),
			html_o=$("<option />"),
			_this=this,
			levelData=_this.areaData.data.items;

		if(levelData){

			if(_this.modeNum>3 || _this.modeNum<1){
				var levelSelect=html_s.clone().append(html_o.clone().val("").html("--����--"));
				_this.idName.append(levelSelect);
				$.each(levelData,function(key,item){
					levelSelect.append(html_o.clone().val(levelData[key].no).html(levelData[key].name));
				})

			}else{

				for(var i=0;i<_this.modeNum;i++){
					var o_text=i==0 ? "--ʡ��--":i==1 ? "--�ؼ���--": i==2 ? "--�ؼ���--":"--��ѡ��--";
					var levelSelect=html_s.clone().append(html_o.clone().val("").html(o_text));
					_this.idName.append(levelSelect);
					if(i=='0'){
						$.each(levelData,function(key,item){
							levelSelect.append(html_o.clone().val(levelData[key].no).html(levelData[key].name));
						})
					}
				}

			}

			$("select",_this.idName).change(function(){
				_this._changeHandler(this);
			});

		}else{
			console.log('���ݼ��س���')
		}

	}
	//������
	areaSelector.prototype._changeHandler = function( select ){
		var $number=$(select).val(),
			_this=this,
			html_o=$("<option />"),
			Level_num=$number.length/4-1;

		if(Level_num=="1"){//ѡ����ʡ
			_this.province($number);
		}else if(Level_num=="2"){//ѡ������
			 _this.city($number);
		}else{
			$(select).nextAll().html(html_o.clone().val("").html("--��ѡ��--"));
		}
		_this.className.val($number);
		_this._currentNo=$number;
	};

	areaSelector.prototype.province=function(num){
		var _this=this,
			html_o=$("<option />"),
			Level_num=num.length/4-1,
			provinceData=_this.areaData.data.items;

		$.each(provinceData,function(key,item){

			if(provinceData[key].no==num){

				$("select:eq("+Level_num+")",_this.idName).html(html_o.clone().val(provinceData[key].no).html("--�ؼ���--"));
				$("select:eq("+Level_num+")",_this.idName).next().html(html_o.clone().val("").html("--�ؼ���--"));
				_this.cityAreaData=provinceData[key].subs;

				$.each(_this.cityAreaData,function(key,item){

					$("select:eq("+Level_num+")",_this.idName).append(html_o.clone().val(_this.cityAreaData[key].no).html(_this.cityAreaData[key].name));

				})

			}
		})

	}

	areaSelector.prototype.city=function(num){
		var _this=this,
			html_o=$("<option />"),
			Level_num=num.length/4-1,
			cityData=_this.cityAreaData;

		$.each(cityData,function(key,item){

			if(cityData[key].no==num){

				$("select:eq("+Level_num+")",_this.idName).html(html_o.clone().val(cityData[key].no).html("--�ؼ���--"));
				var countyData=cityData[key].subs;

				$.each(countyData,function(key,item){
					$("select:eq("+Level_num+")",_this.idName).append(html_o.clone().val(countyData[key].no).html(countyData[key].name));
				})

			}

		})

	}

	//ѡ�к�ķ�����չ
	areaSelector.prototype.change=function(fun){
		if($.isFunction(fun)){
			if(this.isReady){
				$("select").on("change", fun);
			}else{
				this._change=fun;
			}
		}
	}

	//������ֵ��ȡֵ
	areaSelector.prototype.val=function(P_val){
		var _this=this;
		if(P_val === undefined || typeof P_val == "boolean" ){  //Ϊ��ʱ��ȡ��ǰ����
			return _this._currentNo;

		}else if(this.isReady){    //���ݻ�ȡ���
			var val_num=P_val.length,
				val_level=val_num/4,
				reg=/^(0|[1-9][0-9]*)$/;

			if(!reg.test(val_level)){
				return false;
			}
			for(var i=1;i<=val_level;i++){
				var val_str=P_val.substr(0,4*i);
				if(i=="2"){
					_this.province(val_str);
				}else if(i=="3"){
					_this.city(val_str);
				}
				$("[value='"+val_str+"']",_this.idName).attr("selected", "selected");
			}
			_this.className.val(P_val);
			_this._currentNo=P_val;
		}else{//���ݻ�ȡ��
			this._val=P_val;
		}
	}

	window.areaSelect = areaSelector;

})(window)

