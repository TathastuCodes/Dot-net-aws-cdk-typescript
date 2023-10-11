import * as cdk from 'aws-cdk-lib';
import { Construct } from 'constructs';
import * as s3 from 'aws-cdk-lib/aws-s3';
import * as iam from 'aws-cdk-lib/aws-iam';
import * as lambda from 'aws-cdk-lib/aws-lambda';
import * as apigateway from 'aws-cdk-lib/aws-apigateway';
// import * as sqs from 'aws-cdk-lib/aws-sqs';

export class InfraStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props);
    //S3 formation
    // const s3bucket=new s3.Bucket(this,'s3logicalId1234',{
    //   bucketName:'customerbalancestatus2023',
    //   publicReadAccess:false,
    //   versioned:true,
    //   removalPolicy: cdk.RemovalPolicy.DESTROY
    // })

    //IAM Role to be used by  Lambda to access S3
    const iamlambdaaccessrole=new iam.Role(this,'iamlambdaas3role',{
      roleName:'iamlambdas3access',
      assumedBy: new iam.ServicePrincipal('lambda.amazonaws.com'),
      description:'this role will be used by lambda to access s3 content',
      managedPolicies: [
        iam.ManagedPolicy.fromAwsManagedPolicyName(
          'AWSLambda_FullAccess',
        ),
        iam.ManagedPolicy.fromAwsManagedPolicyName(
          'AmazonS3FullAccess',
        )
      ]
    })
    //Lambda declaration
 

    const lambdafn=new lambda.Function(this,'lambdalogicalid',{
      functionName: 'LambdaAccountStatement',
      handler :'LambdaGetCustomerAccountStatement::LambdaGetCustomerAccountStatement.Function::FunctionHandler',
      runtime: lambda.Runtime.DOTNET_6,
      //code: lambda.Code.fromAsset(path.join(__dirname,'../services/LambdaGetCustomerAccountStatement/LambdaGetCustomerAccountStatement/bin/Release/function.zip'))
      code: lambda.Code.fromAsset('../services/LambdaGetCustomerAccountStatement/LambdaGetCustomerAccountStatement/bin/Release/function.zip'),
      role:  iamlambdaaccessrole,
      timeout: cdk.Duration.minutes(5)
    })

    //API To call Lambda
    const bankingapi=new apigateway.LambdaRestApi(this,'bankingapi',{
      handler:lambdafn,
      restApiName:'accountsummery',
      deploy:true,
      proxy:false
    })

    const accounts = bankingapi.root.addResource('accounts');
    accounts.addMethod('GET');  // GET /items //the final api link will be : <invoke url from stage in api gateway>/<resource name>
  }
}
